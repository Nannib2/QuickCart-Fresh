
using BO;
using DO;
using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{

    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers


    private static Task? _periodicTask = null; //stage 7

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// Updates the simulated system clock to a new time.
    /// This function first updates the internal configuration with the new time
    /// and then triggers periodic updates for relevant managers (Delivery and Courier).
    /// It handles potential errors during the periodic updates by catching 'BlDoesNotExistException'.
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4
        ClockUpdatedObservers?.Invoke(); // stage 5
        //Add calls here to any logic method that should be called periodically,
        //after each clock update
        //for example, Periodic students' updates:
        // - Go through all students to update properties that are affected by the clock update
        // - (students become not active after 5 years etc.)
        //TO_DO: //stage 4
        try
        {
            if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
            {
                _periodicTask = Task.Run(async () => {
                    await DeliveryManager.PeriodicUpdates(oldClock, newClock);
                    CourierManager.PeriodicUpdate(newClock);
                });
                //_periodicTask = Task.Run(() => DeliveryManager.PeriodicUpdates(oldClock, newClock));
                //_periodicTask = Task.Run(() => CourierManager.PeriodicUpdate(newClock));
            }

            //await DeliveryManager.PeriodicUpdates(oldClock, newClock); //stage 4. to be removed in stage 7 and replaced as below
            //CourierManager.PeriodicUpdate(newClock); //stage 4. to be removed in stage 7 and replaced as below
        }
        catch (BlDoesNotExistException ex)
        {
           throw new BO.BlDoesNotExistException("Unexpected error in AdminManager.UpdateClock while calling PeriodicUpdates", ex);
        }
    }

    /// <summary>
    /// Retrieves the current system configuration settings from the Data Access Layer (DAL).
    /// It maps relevant configuration properties to a new Business Object (BO) Config instance,
    /// excluding sensitive information like the ManagerPassword for security.
    /// The returned object provides read-only access to system parameters such as the clock, speeds, and delivery rates.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static BO.Config GetConfig() //stage 4
    => new BO.Config()
    {
       Clock = s_dal.Config.Clock,
       ManagerId = s_dal.Config.ManagerId,
       ManagerPassword = s_dal.Config.ManagerPassword,
       AddressCompany = s_dal.Config.AddressCompany,
       Latitude = s_dal.Config.Latitude,
       Longitude = s_dal.Config.Longitude,
       MaxAirDeliveryDistanceKm = s_dal.Config.MaxAirDeliveryDistanceKm,
       AverageMotorcycleSpeedKmh = s_dal.Config.AverageMotorcycleSpeedKmh,
       AverageBicycleSpeedKmh = s_dal.Config.AverageBicycleSpeedKmh,
       AverageCarSpeedKmh = s_dal.Config.AverageCarSpeedKmh,
       AverageWalkingSpeedKmh = s_dal.Config.AverageWalkingSpeedKmh,
       MaxDeliveryTimeRange = s_dal.Config.MaxDeliveryTimeRange,
       RiskTimeRange = s_dal.Config.RiskTimeRange,
       InactivityTimeRange = s_dal.Config.InactivityTimeRange,
       BaseSalaryMounthly= s_dal.Config.BaseSalaryMounthly,
       DeliveryRatePerKm= s_dal.Config.DeliveryRatePerKm,
    };

    /// <summary>
    /// Updates the system configuration by applying the provided BO.Config object.
    /// It rigorously validates every property (e.g., IDs, password strength, coordinates, speed ranges, and time spans)
    /// and updates the underlying Data Access Layer (DAL) configuration only if the new values are valid and different from the current ones.
    /// </summary>
    
   //מי[MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static async Task SetConfig(BO.Config configuration) //stage 4
    {
        try
        {
            AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

            bool configChanged = false; // stage 5

            if (s_dal.Config.Clock != configuration.Clock) //stage 4
            {
                s_dal.Config.Clock = configuration.Clock;
                configChanged = true;
            }
            if (s_dal.Config.ManagerId != configuration.ManagerId) //stage 4
            {
                if (Tools.IsValidIsraeliId(configuration.ManagerId) == false) //stage 4
                    throw new BlInvalidInputException("Manager ID is not a valid Israeli ID"); //stage 4
                s_dal.Config.ManagerId = configuration.ManagerId;
                configChanged = true;
            }
           
            if (configuration.ManagerPassword != s_dal.Config.ManagerPassword/*!Tools.ManageStrongPassword.VerifyPassword(configuration.ManagerPassword, s_dal.Config.ManagerPassword)*/) //make sure the password is different //stage 4
            {
                if (Tools.ManageStrongPassword.IsPasswordStrong(configuration.ManagerPassword) == false)// check password strength //stage 4
                    throw new BlInvalidInputException("Manager Password is not strong enough"); //stage 4
                s_dal.Config.ManagerPassword = Tools.ManageStrongPassword.HashPassword(configuration.ManagerPassword);// save the hashed password //stage 4
                configChanged = true;
            }
            if (s_dal.Config.AddressCompany != configuration.AddressCompany && configuration.AddressCompany!=null) //stage 4
            {
                try
                {
                     await Tools.CalculateCoordinate(configuration.AddressCompany); // validate address //stage 4
                    s_dal.Config.AddressCompany = configuration.AddressCompany;
                    configChanged = true;
                }
                catch (BlInvalidInputException ex)
                {
                    throw new BlInvalidInputException("Company Address is not valid", ex); //stage 4
                }

            }
            if (s_dal.Config.Latitude != configuration.Latitude) //stage 4
            {
                if (!(configuration.Latitude >= -90 && configuration.Latitude <= 90))
                    throw new BlInvalidInputException("Latitude must be between -90 and 90"); //stage 4
                s_dal.Config.Latitude = configuration.Latitude;
                configChanged = true;
            }
            if (s_dal.Config.Longitude != configuration.Longitude) //stage 4
            {
                if (!(configuration.Longitude >= -180 && configuration.Longitude <= 180))
                    throw new BlInvalidInputException("Longitude must be between -90 and 90"); //stage 4
                s_dal.Config.Longitude = configuration.Longitude;
                configChanged = true;
            }
            if (s_dal.Config.MaxAirDeliveryDistanceKm != configuration.MaxAirDeliveryDistanceKm) //stage 4
            {
                if (configuration.MaxAirDeliveryDistanceKm <= 0 || configuration.MaxAirDeliveryDistanceKm > 50)
                    throw new BlInvalidInputException("Max Air Delivery Distance must be positive and under 50"); //stage 4
                s_dal.Config.MaxAirDeliveryDistanceKm = configuration.MaxAirDeliveryDistanceKm;
                configChanged = true;
            }
            if (s_dal.Config.AverageMotorcycleSpeedKmh != configuration.AverageMotorcycleSpeedKmh) //stage 4
            {
                if (configuration.AverageMotorcycleSpeedKmh <= 0 || configuration.AverageMotorcycleSpeedKmh > 80)
                    throw new BlInvalidInputException("Average Motorcycle Speed must be positive and under 80"); //stage 4
                s_dal.Config.AverageMotorcycleSpeedKmh = configuration.AverageMotorcycleSpeedKmh;
                configChanged = true;
            }
            if (s_dal.Config.AverageBicycleSpeedKmh != configuration.AverageBicycleSpeedKmh) //stage 4
            {
                if (configuration.AverageBicycleSpeedKmh <= 0 || configuration.AverageBicycleSpeedKmh > 25)
                    throw new BlInvalidInputException("Average Bicycle Speed must be positive and under 25"); //stage 4
                s_dal.Config.AverageBicycleSpeedKmh = configuration.AverageBicycleSpeedKmh;
                configChanged = true;
            }
            if (s_dal.Config.AverageCarSpeedKmh != configuration.AverageCarSpeedKmh) //stage 4
            {
                if (configuration.AverageCarSpeedKmh <= 0 || configuration.AverageCarSpeedKmh > 90)
                    throw new BlInvalidInputException("Average Car Speed must be positive and under 90"); //stage 4
                s_dal.Config.AverageCarSpeedKmh = configuration.AverageCarSpeedKmh;
                configChanged = true;
            }
            if (s_dal.Config.AverageWalkingSpeedKmh != configuration.AverageWalkingSpeedKmh) //stage 4
            {
                if (configuration.AverageWalkingSpeedKmh <= 0 || configuration.AverageWalkingSpeedKmh > 8)
                    throw new BlInvalidInputException("Average Walking Speed must be positive and under 8"); //stage 4
                s_dal.Config.AverageWalkingSpeedKmh = configuration.AverageWalkingSpeedKmh;
                configChanged = true;
            }
            if (s_dal.Config.MaxDeliveryTimeRange != configuration.MaxDeliveryTimeRange) //stage 4
            {
                if (configuration.MaxDeliveryTimeRange < TimeSpan.FromHours(1) || configuration.MaxDeliveryTimeRange > TimeSpan.FromDays(21))
                    throw new BO.BlInvalidInputException("MaxDeliveryTimeRange must be between 1 hour and 21 days.");

                s_dal.Config.MaxDeliveryTimeRange = configuration.MaxDeliveryTimeRange;
                configChanged = true;
            }
            if (s_dal.Config.RiskTimeRange != configuration.RiskTimeRange) //stage 4
            {
                if (configuration.MaxDeliveryTimeRange < TimeSpan.FromHours(2) || configuration.MaxDeliveryTimeRange > TimeSpan.FromDays(14))
                    throw new BO.BlInvalidInputException("RiskTimeRange must be between 2 hours and 14 days.");
                s_dal.Config.RiskTimeRange = configuration.RiskTimeRange;
                configChanged = true;
            }
            if (s_dal.Config.InactivityTimeRange != configuration.InactivityTimeRange) //stage 4
            {
                if (configuration.InactivityTimeRange < TimeSpan.FromDays(14) || configuration.InactivityTimeRange > TimeSpan.FromDays(90))
                    throw new BO.BlInvalidInputException("InactivityTimeRange must be between 14 days and 90 days.");
                s_dal.Config.InactivityTimeRange = configuration.InactivityTimeRange;
                configChanged = true;
            }
            if (s_dal.Config.BaseSalaryMounthly != configuration.BaseSalaryMounthly) //stage 4
            {
                if (configuration.BaseSalaryMounthly < 0 || configuration.BaseSalaryMounthly > 10000)
                    throw new BO.BlInvalidInputException("Base Salary Monthly must be non-negative and below 10000.");
                s_dal.Config.BaseSalaryMounthly = configuration.BaseSalaryMounthly;
                configChanged = true;
            }
            if (s_dal.Config.DeliveryRatePerKm != configuration.DeliveryRatePerKm) //stage 4
            {
                if (configuration.DeliveryRatePerKm < 0 || configuration.DeliveryRatePerKm > 50)
                    throw new BO.BlInvalidInputException("Delivery Rate Per Km must be non-negative and below 50.");
                s_dal.Config.DeliveryRatePerKm = configuration.DeliveryRatePerKm;
                configChanged = true;
            }


            //Calling all the observers of configuration update
            if (configChanged) // stage 5
                ConfigUpdatedObservers?.Invoke(); // stage 5
        }
        catch (BO.BlInvalidInputException ex)
        {
            throw new BO.BlInvalidInputException("Invalid configuration value " + ex.Message);
        }

    }
    internal static void ForwardClock(TimeUnit timeUnit)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        switch (timeUnit)
        {
            case TimeUnit.Minute:
                 AdminManager.UpdateClock(AdminManager.Now.AddMinutes(1));
                break;
            case TimeUnit.Hour:
                 AdminManager.UpdateClock(AdminManager.Now.AddHours(1));
                break;
            case TimeUnit.Day:
                AdminManager.UpdateClock(AdminManager.Now.AddDays(1));
                break;
            case TimeUnit.Month:
                AdminManager.UpdateClock(AdminManager.Now.AddMonths(1));
                break;
            case TimeUnit.Year:
                AdminManager.UpdateClock(AdminManager.Now.AddYears(1));
                break;

        }
        ClockUpdatedObservers?.Invoke(); // stage 5
    }

    /// <summary>
    /// Resets the entire data layer (database) to its initial, empty state.
    /// This operation is performed under a lock (BlMutex) to ensure thread safety during the reset.
    /// After the reset, it immediately updates the system clock and configuration to refresh the Presentation Layer (PL).
    /// </summary>
    internal static void ResetDB() //stage 4-7
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
             AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            //AdminManager.SetConfig(AdminManager.GetConfig()); //stage 5 - needed to update PL 
        }
        ConfigUpdatedObservers?.Invoke();
    }

    /// <summary>
    /// Initializes the data layer by loading predefined test data.
    /// This operation executes within a thread-safe lock and delegates the data generation to the DalTest module.
    /// It includes robust error handling for various DAL failures and updates the clock and configuration after successful initialization.
    /// </summary>
    /// <exception cref="BlFailedToGenerate"></exception>
    /// <exception cref="BlXMLFileLoadCreateException"></exception>
    /// <exception cref="FormatException"></exception>
    internal static void InitializeDB() //stage 4-7
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        try
            {
            lock (BlMutex) //stage 7
            {
                DalTest.Initialization.Do(); //stage 4
                AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated    
             }
                ConfigUpdatedObservers?.Invoke();
            }
            catch (DalFailedToGenerate ex)
            {
                throw new BlFailedToGenerate(($"Initialization Error (Data Generation): {ex.Message}"));
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new BO.BlXMLFileLoadCreateException($"Initialization Error (DAL Operation): {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Initialization Error (Format Conversion): {ex.Message}"); 
            }
                  
            //AdminManager.SetConfig(AdminManager.GetConfig()); //stage 5 - needed for update the PL
           
        
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;
    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())    
    /// </summary>
    private static int s_interval = 1;
    /// <summary>
    /// The flag that signs whether simulator is running
    /// 
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BLTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    private static Task? _simulateTask = null;

    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation
            if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
                _simulateTask = Task.Run(() => CourierManager.SimulateCourierActivityAsync());

            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }

    #endregion Stage 7 base
}
