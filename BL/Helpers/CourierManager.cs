using BLApi;
using BO;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Helpers;

internal static class CourierManager
{
    private static readonly IDal s_dal = DalApi.Factory.Get;
    /// <summary>
    /// Manages the observers for courier-related updates, notifying subscribers (like PL) when changes occur.
    /// </summary>
    internal static ObserverManager Observers = new(); //stage 5 
    private static readonly AsyncMutex s_periodicMutex = new(); //stage 7
    private static readonly AsyncMutex s_simulationMutex = new(); //stage 7
    private static readonly Random s_rand = new();


    /// <summary>
    /// Retrieves the full Business Object (BO) details for a specific courier using their ID.
    /// It fetches the Data Object (DO) from the DAL, maps its properties, and calculates derived metrics like on-time/late delivery counts and salary.
    /// The function throws a BlDoesNotExistException if no courier with the specified ID is found in the data layer.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static async Task<BO.Courier> GetCourier(int id)
    {
        DO.Courier doCourier;

        // Step 1: Read basic data inside lock
        lock (AdminManager.BlMutex)
        {
            doCourier = s_dal.Courier.Read(id) ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does Not exist");
        }

        // Step 2: Async call MUST be outside lock
        var activeOrder = await GetCourierActiveOrder(doCourier.Id);

        // Step 3: Calculate derived properties (accesses DAL again, so needs lock or internal methods handle locks)
        // Since OnTimeDelivery/LateDelivery/CalculateCourierSalary access DAL, we need to ensure thread safety.
        // Option A: Lock around them here. Option B: Make them thread-safe internally.
        // Below, I made the helper methods thread-safe internally (Re-entrant locks work fine).

        BO.Courier boCourier;
        lock (AdminManager.BlMutex)
        {
            // Re-read or use existing DO data (safe if ID doesn't change)
            // Recalculating stats needs DAL access

            boCourier = new BO.Courier()
            {
                Id = doCourier.Id,
                StartDate = doCourier.EmploymentStartDateTime,
                NameCourier = doCourier.NameCourier,
                PhoneNumber = doCourier.PhoneNumber,
                EmailCourier = doCourier.EmailCourier,
                IsActive = doCourier.Active,
                PasswordCourier = doCourier.PasswordCourier,
                DeliveryType = (BO.DeliveryTypeMethods)doCourier.CourierDeliveryType,
                PersonalMaxAirDistance = doCourier.PersonalMaxAirDistance,
                // These methods below will handle their own locks internally
                TotalDeliveredOnTime = OnTimeDelivery(id),
                TotalLateDeliveries = LateDelivery(id),
                OrderInProgress = activeOrder, // Already fetched async
                SalaryForCourier = null
            };

            // CalculateSalary accesses Config and Delivery DAL, needs to be safe.
            // We'll allow CalculateCourierSalary to handle its internal locking or wrap it here.
            // Since CalculateCourierSalary is complex, let's look at its implementation below.
            boCourier.SalaryForCourier = CalculateCourierSalary(boCourier);
        }

        return boCourier;
    }

    /// <summary>
    /// Retrieves a simplified summary of a specific courier suitable for display in a list.
    /// It fetches the Courier Data Object (DO) by ID and maps a subset of its properties to the Business Object (BO) list format.
    /// The function calculates key performance indicators like on-time and late deliveries, and also identifies any currently active delivery ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    internal static BO.CourierInList? GetCourierInList(int id)
    {
        lock (AdminManager.BlMutex)
        {
            DO.Courier doCourier;
            doCourier = s_dal.Courier.Read(id) ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does Not exist");
            return new BO.CourierInList()
            {
                Id = doCourier.Id,
                NameCourier = doCourier.NameCourier,
                IsActive = doCourier.Active,
                DeliveryType = (BO.DeliveryTypeMethods)doCourier.CourierDeliveryType,
                WorkStartTime = doCourier.EmploymentStartDateTime,
                OnTimeDeliveries = OnTimeDelivery(doCourier.Id),
                LateDeliveries = LateDelivery(doCourier.Id),
                ActiveDeliveryId = IsActiveDelivery(doCourier.Id),
            };
        }
    }

    /// <summary>
    /// Performs periodic updates on courier statuses based on the elapsed time relative to the new system clock.
    /// This function identifies couriers who do not have an active delivery and checks their last delivery completion time.
    /// Couriers who have been inactive longer than the configured 'InactivityTimeRange' are automatically marked as inactive in the DAL.
    /// </summary>
    /// <param name="newClock"></param>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    /// <exception cref="BO.BlNullPropertyException"></exception>
    internal static void PeriodicUpdate(DateTime newClock)
    {
        try
        {
            if (s_periodicMutex.CheckAndSetInProgress())//check if another periodic update is running
                return;
            List<int> couriersUpdatedIds = new();
            List<int> activeCouriersIds = new();

            List<DO.Courier> allCouriers;
            List<DO.Delivery> allDeliveries;
            TimeSpan inactivityRange;

            // Step 1: Load all required data inside lock
            lock (AdminManager.BlMutex)
            {
                allCouriers = s_dal.Courier.ReadAll().ToList();
                allDeliveries = s_dal.Delivery.ReadAll().ToList();
                inactivityRange = s_dal.Config.InactivityTimeRange;
            }

            // Step 2: Identify couriers without active deliveries
            var couriersWithoutActiveDelivery = allCouriers
                .Where(courier => !allDeliveries.Any(d => d.CourierId == courier.Id && d.OrderEndDateTime == null))
                .ToList();

            //  Find inactive couriers based on last completed delivery
            var inactiveCouriers = couriersWithoutActiveDelivery
                .Select(courier =>
                {
                    var lastDelivery = allDeliveries
                        .Where(d => d.CourierId == courier.Id && d.OrderEndDateTime != null)
                        .OrderByDescending(d => d.OrderEndDateTime)
                        .FirstOrDefault();
                    return new { Courier = courier, LastCompletedDelivery = lastDelivery };
                })
                .Where(x => x.LastCompletedDelivery != null)
                .Where(x => (newClock - x.LastCompletedDelivery!.OrderEndDateTime) > inactivityRange)
                .ToList();

            //  Update inactive couriers inside lock
            lock (AdminManager.BlMutex)
            {
                foreach (var item in inactiveCouriers)
                {
                    var updatedCourier = item.Courier with { Active = false };
                    s_dal.Courier.Update(updatedCourier);
                    couriersUpdatedIds.Add(updatedCourier.Id);
                }
            }
            foreach (var id in couriersUpdatedIds)
            {
                Observers.NotifyItemUpdated(id);
                Observers.NotifyListUpdated();
            }
        }
        catch (DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error during periodic courier update.", ex);
        }
        catch (BlNullPropertyException ex)
        {
            throw new BO.BlNullPropertyException("BlNullProperty: " + ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlNullPropertyException("Handling LINQ error: " + ex.Message);
        }
        finally
        {
            s_periodicMutex.UnsetInProgress();
        }
    }


    /// <summary>
    /// Checks if the specified courier currently has an active order in progress.
    /// It searches the DAL for a delivery assigned to the courier that has not yet been completed (DeliveryTypeEnding is null).
    /// If an active delivery is found, it calls the OrderManager to retrieve and return the full BO.OrderInProgress object; otherwise, it returns null.
    /// </summary>
    /// <param name="idCourier"></param>
    /// <returns></returns>
    internal static async Task<BO.OrderInProgress?> GetCourierActiveOrder(int idCourier)
    {
        try
        {
            DO.Delivery? doDelivery;
            // Lock read from DAL
            lock (AdminManager.BlMutex)
            {
                doDelivery = s_dal.Delivery.Read(delivery => delivery.CourierId == idCourier && delivery.DeliveryTypeEnding == null);
            }

            if (doDelivery == null)
                return null;

            // Async call outside lock
            return await OrderManager.GetOrderInProgress(idCourier);
        }
        catch (BO.BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error retrieving active order for courier." + ex.Message);
        }
    }

    /// <summary>
    /// Counts the total number of deliveries completed by a specific courier that were supplied on time.
    /// A delivery is considered on time if its completion time (OrderEndDateTime - OrderStartDateTime) is less than or equal to the configured MaxDeliveryTimeRange.
    /// This count only includes deliveries that were successfully completed (DeliveryTypeEnding is Supplied).
    /// </summary>
    /// <param name="idCourier"></param>
    /// <returns></returns>
    internal static int OnTimeDelivery(int idCourier)
    {
        lock (AdminManager.BlMutex)
        {
            TimeSpan maxRange = s_dal.Config.MaxDeliveryTimeRange;
            return s_dal.Delivery.ReadAll(d => d.CourierId == idCourier &&
                                          d.DeliveryTypeEnding == DO.DeliveryCompletionType.Supplied &&
                                          (d.OrderEndDateTime - d.OrderStartDateTime) <= maxRange)
                                 .Count();
        }
    }

    /// <summary>
    /// Counts the total number of deliveries completed by a specific courier that were supplied late.
    /// A delivery is considered late if its completion time (OrderEndDateTime - OrderStartDateTime) is greater than the configured MaxDeliveryTimeRange.
    /// This count only includes deliveries that were successfully completed (DeliveryTypeEnding is Supplied).
    /// </summary>
    /// <param name="idCourier"></param>
    /// <returns></returns>
    internal static int LateDelivery(int idCourier)
    {
        lock (AdminManager.BlMutex)
        {
            TimeSpan maxRange = s_dal.Config.MaxDeliveryTimeRange;
            return s_dal.Delivery.ReadAll(d => d.CourierId == idCourier &&
                                          d.DeliveryTypeEnding == DO.DeliveryCompletionType.Supplied &&
                                          (d.OrderEndDateTime - d.OrderStartDateTime) > maxRange)
                                 .Count();
        }
    }

    /// <summary>
    /// Checks for and returns the ID of a delivery that is currently active for the specified courier.
    /// It queries the data layer for a delivery assigned to the courier that does not yet have a recorded end time (DeliveryTypeEnding is null).
    /// If an active delivery is found, its ID is returned as an integer; otherwise, the function returns null.
    /// </summary>
    /// <param name="idCourier"></param>
    /// <returns></returns>
    internal static int? IsActiveDelivery(int idCourier)
    {
        DO.Delivery? doDelivery;
        lock (AdminManager.BlMutex)
            doDelivery = s_dal.Delivery.Read(delivery => delivery.CourierId == idCourier && delivery.DeliveryTypeEnding == null);//only one active delivery per courier
        if (doDelivery == null)
            return null;
        return doDelivery.Id;
    }

    /// <summary>
    /// Validates the data integrity and constraints of a Business Object (BO) Courier instance.
    /// It performs checks on the courier's identifying information (ID, Email, Phone Number) using specific utility functions.
    /// It also verifies constraints on delivery parameters (PersonalMaxAirDistance) and ensures the security requirements for the password are met.
    /// </summary>
    /// <param name="boCourier"></param>
    /// <exception cref="BO.BlInvalidInputException"></exception>
    internal static void IsValidData(BO.Courier boCourier, string? currentDbHash = null)
    {
        if (!Tools.IsValidIsraeliId(boCourier.Id))
        {
            throw new BO.BlInvalidInputException("Invalid Israeli ID number.");
        }
        if (!IsValidEmailFormat(boCourier.EmailCourier))
        {
            throw new BO.BlInvalidInputException("Invalid Email format.");
        }
        if (!Tools.IsValidMobileNumber(boCourier.PhoneNumber))
        {
            throw new BO.BlInvalidInputException("Invalid Phone Number format.");
        }
        if (boCourier.PersonalMaxAirDistance != null && boCourier.PersonalMaxAirDistance < 0)
        {
            throw new BO.BlInvalidInputException("Personal max air distance cannot be negative.");
        }
        if (boCourier.PersonalMaxAirDistance != null)
        {
            double? maxDistance;
            lock (AdminManager.BlMutex)
            {
                maxDistance = s_dal.Config.MaxAirDeliveryDistanceKm;
            }

            if (boCourier.PersonalMaxAirDistance > maxDistance)
                throw new BO.BlInvalidInputException("Personal max air distance cannot be bigger than the maximum of the company.");
        }
        if (currentDbHash == null || boCourier.PasswordCourier != currentDbHash)
            if (!Tools.ManageStrongPassword.IsPasswordStrong(boCourier.PasswordCourier))
            {
                throw new BO.BlInvalidInputException("Password is not strong enough.");
            }
       
    }

    /// <summary>
    /// Creates a new courier entity in the system after validating the request and the provided data.
    /// It first checks if the 'requesterId' matches the system's Manager ID for authorization, throwing an exception if access is denied.
    /// If authorized, it validates the BO.Courier data, maps it to a DO.Courier object (hashing the password), sets the start date, and persists it to the DAL, handling existence and validation exceptions.
    /// </summary>
    /// <param name="boCourier"></param>
    /// <param name="requesterId"></param>
    /// <exception cref="BO.BlAlreadyExistsException"></exception>
    /// <exception cref="BlInvalidInputException"></exception>
    /// <exception cref="BlInvalidOperationException"></exception>
    internal static void GetCreateCourier(BO.Courier boCourier, int requesterId)
    {
        try
        {
            

            lock (AdminManager.BlMutex)
            {
                if (requesterId == s_dal.Config.ManagerId)
                {
                    // IsValidData locks internally, so we have re-entrant lock here, which is fine
                    IsValidData(boCourier);

                    DO.Courier doCourier = new DO.Courier()
                    {
                        Id = boCourier.Id,
                        NameCourier = boCourier.NameCourier,
                        PhoneNumber = boCourier.PhoneNumber,
                        EmailCourier = boCourier.EmailCourier,
                        PasswordCourier = Tools.ManageStrongPassword.HashPassword(boCourier.PasswordCourier),
                        Active = boCourier.IsActive,
                        PersonalMaxAirDistance = boCourier.PersonalMaxAirDistance,
                        CourierDeliveryType = (DO.DeliveryTypeMethods)boCourier.DeliveryType,
                        EmploymentStartDateTime = AdminManager.Now,
                    };
                    s_dal.Courier.Create(doCourier);
                }
                else
                {
                    throw new BlInvalidOperationException("This user does not have access permission.");
                }
            }
            // Notification outside lock
            Observers.NotifyListUpdated();
        }
        catch (DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("Courier with the same ID already exists." + ex.Message, ex);
        }
        catch (BlInvalidInputException ex)
        {
            throw new BlInvalidInputException("Invalid input data for creating courier." + ex.Message, ex);
        }
        catch (BlInvalidOperationException ex)
        {
            throw new BlInvalidOperationException(ex.Message);
        }
    }


    /// <summary>
    /// Validates if a string is a correctly formatted email address.
    /// Validates if the provided string adheres to a standard, comprehensive email format.
    /// The function first checks for null or empty input, then relies on the System.Net.Mail.MailAddress class constructor for rigorous structural validation.
    /// It returns true if the format is valid (single '@', valid local and domain parts) and false if a FormatException or other validation errors occur.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <returns>True if the email is valid, false otherwise.</returns>
    /// using AI assistance
    /// https://gemini.google.com/share/f1778b24a6cb
    internal static bool IsValidEmailFormat(string email)
    {
        // 1. Check for null or empty input
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            // 2. Use the MailAddress class for comprehensive format validation
            // This checks for:
            // - A single '@' symbol.
            // - Valid character sets in the local and domain parts.
            // - A valid domain structure (e.g., domain.tld).
            //expect it to throw FormatException if invalid
            var addr = new MailAddress(email);

            // 3. Optional: Check that the local part and domain are not empty strings 
            // (already partially covered by MailAddress, but adds robustness).
            if (string.IsNullOrWhiteSpace(addr.User) || string.IsNullOrWhiteSpace(addr.Host))
            {
                return false;
            }

            // The MailAddress constructor succeeded, so the format is valid.
            return true;
        }
        catch (FormatException)
        {
            // The format is invalid (e.g., missing '@', invalid characters, etc.)
            return false;
        }
        catch (Exception)
        {
            // Catch any other unexpected exceptions.
            return false;
        }
    }


    /// <summary>
    /// Attempts to permanently remove a courier entity from the data system using their ID.
    /// The operation employs a two-phase locking strategy:
    /// 1. A short lock to retrieve configuration data for permission verification.
    /// 2. A separate lock to perform the transactional check-and-delete operation, ensuring data integrity 
    ///    while minimizing the critical section duration.
    /// </summary>
    /// <param name="courierId">The unique identifier of the courier to delete.</param>
    /// <param name="requesterId">The ID of the user requesting the deletion (must be Manager).</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier does not exist.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when the requester is not the manager or if the courier has dependent deliveries.</exception>
    internal static void DeleteCourier(int courierId, int requesterId)
    {
        try
        {
            int managerId;

            //  Critical Section: Retrieve Configuration
            // Minimal lock to fetch the Manager ID safely.
            lock (AdminManager.BlMutex)
            {
                managerId = s_dal.Config.ManagerId;
            }

            //  Permission Check
            // Performed outside the lock to avoid blocking resources for unauthorized requests.
            if (requesterId != managerId)
            {
                throw new BlInvalidOperationException("This user does not have access permission.");
            }


            //  Check Existence and Delete
            lock (AdminManager.BlMutex)
            {
                // Verify existence
                if (s_dal.Courier.Read(courierId) is null)
                    throw new BO.BlDoesNotExistException($"Courier with ID={courierId} does Not exist");

                bool hasDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courierId).Any();

                if (hasDeliveries)
                {
                    throw new BO.BlInvalidOperationException("Cannot delete courier with existing deliveries.");
                }

                // Perform Deletion
                s_dal.Courier.Delete(courierId);
            }

            // Notifications 
            Observers.NotifyListUpdated();
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlNullPropertyException("Handling LINQ error: " + ex.Message);
        }
    }

    /// <summary>
    /// Updates the details of an existing courier after validating the input data and user permissions.
    /// The update is authorized for the system manager or the courier themselves; certain fields like ID and Start Date cannot be changed.
    /// It verifies the validity of the updated data (e.g., password strength) and persists the changes to the DAL, handling existence and permission exceptions.
    /// </summary>
    /// <param name="boCourier"></param>
    /// <param name="requesterId"></param>
    /// <exception cref="BlDoesNotExistException"></exception>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    /// <exception cref="BlInvalidOperationException"></exception>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    /// <exception cref="BlInvalidInputException"></exception>
    /// <summary>
    /// Updates the details of an existing courier in the system.
    /// This method handles permission validation, business rule verification (such as immutable fields),
    /// and secure password updating.
    /// The operation employs a fine-grained locking mechanism: data retrieval (including configuration constants) 
    /// and persistence are synchronized, while heavy operations like password hashing are performed asynchronously.
    /// </summary>
    /// <param name="boCourier">The business object containing the updated courier details.</param>
    /// <param name="requesterId">The ID of the user requesting the update (used for permission checks).</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the courier to be updated is not found in the database.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown when attempting to modify immutable fields or when the requester lacks necessary permissions.</exception>
    /// <exception cref="BO.BlInvalidInputException">Thrown when the input data fails validation rules.</exception>
    internal static void UpdateCourier(BO.Courier boCourier, int requesterId)
    {
        try
        {
            DO.Courier? existingCourier;
            int managerId;

            // 1. Critical Section: Read Data
            // Acquire lock to safely fetch courier data AND configuration data (ManagerId)
            lock (AdminManager.BlMutex)
            {
                existingCourier = s_dal.Courier.Read(boCourier.Id);
                managerId = s_dal.Config.ManagerId; // Fetching shared config inside lock
            }

            // Validate existence outside the lock
            if (existingCourier == null)
            {
                throw new BO.BlDoesNotExistException($"Courier with ID={boCourier.Id} does Not exist");
            }

            // 2. Heavy Processing: Logic & Hashing (No Lock)

            IsValidData(boCourier, existingCourier.PasswordCourier);

            string passwordToSave = existingCourier.PasswordCourier;

            // Permission and Logic Checks using the local 'managerId' variable
            if (requesterId == managerId || requesterId == existingCourier.Id)
            {
                // Verify immutable fields
                if (existingCourier.Id != boCourier.Id)
                    throw new BO.BlInvalidOperationException("Cannot change the ID of an existing courier.");

                if (existingCourier.EmploymentStartDateTime != boCourier.StartDate)
                    throw new BO.BlInvalidOperationException("Cannot change the Employment Start Date of an existing courier.");

                // Handle Password Hashing (CPU intensive)
                if (existingCourier.PasswordCourier != boCourier.PasswordCourier)
                {
                    passwordToSave = Tools.ManageStrongPassword.HashPassword(boCourier.PasswordCourier);
                }
            }
            else
            {
                // Permission check for non-owners/non-managers
                if (boCourier.IsActive != existingCourier.Active && requesterId != managerId)
                    throw new BO.BlInvalidOperationException("Only managers can change the active status of a courier.");

                if (requesterId != managerId && requesterId != existingCourier.Id)
                    throw new BO.BlInvalidOperationException("This user does not have access permission.");
            }

            // Prepare the updated DO object
            DO.Courier updatedCourier = existingCourier with
            {
                NameCourier = boCourier.NameCourier,
                PhoneNumber = boCourier.PhoneNumber,
                EmailCourier = boCourier.EmailCourier,
                PasswordCourier = passwordToSave,
                Active = boCourier.IsActive,
                PersonalMaxAirDistance = boCourier.PersonalMaxAirDistance,
                CourierDeliveryType = (DO.DeliveryTypeMethods)boCourier.DeliveryType,
            };

            // 3. Critical Section: Write Data
            lock (AdminManager.BlMutex)
            {
                s_dal.Courier.Update(updatedCourier);
            }

            // 4. Notifications
            Observers.NotifyItemUpdated(boCourier.Id);
            Observers.NotifyListUpdated();
        }
        catch (DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Courier does not exist. " + ex.Message, ex);
        }
        catch (BlInvalidInputException ex)
        {
            throw new BO.BlInvalidInputException("Invalid input data for updating courier. " + ex.Message, ex);
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlNullPropertyException("Handling LINQ error: " + ex.Message);
        }
    }
    /// <summary>
    /// Retrieves the full Business Object (BO) details for a specific courier by ID.
    /// Access is strictly controlled, permitting only the system manager or the courier themselves (requesterId matching courierId).
    /// Throws a BlInvalidOperationException if the requester lacks permission or a BlDoesNotExistException if the courier ID is not found.
    /// </summary>
    /// <param name="courierId"></param>
    /// <param name="requesterId"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    /// <exception cref="BlInvalidOperationException"></exception>
    internal static async Task<BO.Courier> ReadCourier(int courierId, int requesterId)
    {
        bool hasPermission = false;

        lock (AdminManager.BlMutex)
        {
            if (requesterId == s_dal.Config.ManagerId || requesterId == courierId)
            {
                hasPermission = true;
            }
        }

        if (hasPermission)
        {
            // GetCourier handles its own locks and async correctly
            return await GetCourier(courierId) ?? throw new BO.BlDoesNotExistException($"Courier with ID={courierId} does Not exist");
        }
        else
        {
            throw new BlInvalidOperationException("This user does not have access permission.");
        }
    }

    /// <summary>
    /// Retrieves a list of all couriers, allowing for complex filtering and sorting, but only for authorized managers.
    /// It reads all couriers from the DAL, converts them to BO.CourierInList objects, and then filters based on the 'active' status and optional 'filterBy' properties.
    /// Finally, the resulting list is sorted according to the specified 'sortBy' criteria before being returned as an enumerable collection.
    /// </summary>
    /// <param name="requesterId"></param>
    /// <param name="active"></param>
    /// <param name="sortBy"></param>
    /// <param name="filterBy"></param>
    /// <returns></returns>
    /// <exception cref="BlDoesNotExistException"></exception>
    /// <exception cref="BlInvalidOperationException"></exception>
    internal static IEnumerable<BO.CourierInList> ReadAllCouriers(int requesterId, bool? active, CourierInListProperties? sortBy, CourierInListProperties? filterBy, Object? innerFilterBy)
    {
        try
        {
            IEnumerable<BO.CourierInList> boCouriers;

            lock (AdminManager.BlMutex)
            {
                if (requesterId != s_dal.Config.ManagerId)
                {
                    throw new BlInvalidOperationException("This user does not have access permission.");
                }

                var dalCouriers = s_dal.Courier.ReadAll().ToList(); // Materialize inside lock
                if (!dalCouriers.Any()) throw new BlDoesNotExistException("there aren't couirers");
                boCouriers = dalCouriers.ToBoCourierInList().ToList();
            }

            // Filtering and sorting can happen outside the lock on the in-memory list
            if (active != null)
            {
                boCouriers = boCouriers.Where(c => c.IsActive == active);
            }
            if (filterBy != null)
            {
                boCouriers = filterBy switch
                {
                    CourierInListProperties.DeliveryType =>
                        innerFilterBy == null ? boCouriers
                        : boCouriers.Where(c => c.DeliveryType == (BO.DeliveryTypeMethods)innerFilterBy),

                    CourierInListProperties.OnTimeDeliveries =>
                        boCouriers.Where(c => c.OnTimeDeliveries > 0),

                    CourierInListProperties.WorkStartTime =>
                        boCouriers.Where(c => c.WorkStartTime.Year == AdminManager.Now.Year), // using local property for current year/clock? Better use AdminManager.Now which reads config (locked)

                    CourierInListProperties.LateDeliveries =>
                        boCouriers.Where(c => c.LateDeliveries > 0),

                    CourierInListProperties.ActiveDeliveryId =>
                        boCouriers.Where(c => c.ActiveDeliveryId != null),

                    _ => boCouriers
                };
            }

            boCouriers = sortBy switch
            {
                null => boCouriers.OrderBy(c => c.Id),
                CourierInListProperties.NameCourier => boCouriers.OrderBy(c => c.NameCourier),
                CourierInListProperties.DeliveryType => boCouriers.OrderBy(c => c.DeliveryType),
                CourierInListProperties.OnTimeDeliveries => boCouriers.OrderBy(c => c.OnTimeDeliveries),
                CourierInListProperties.WorkStartTime => boCouriers.OrderBy(c => c.WorkStartTime),
                CourierInListProperties.LateDeliveries => boCouriers.OrderBy(c => c.LateDeliveries),
                CourierInListProperties.IsActive => boCouriers.OrderBy(c => c.IsActive),
                CourierInListProperties.ActiveDeliveryId => boCouriers.OrderBy(c => c.ActiveDeliveryId),
                _ => boCouriers.OrderBy(c => c.Id)
            };

            return boCouriers;
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlNullPropertyException("handaling LINQ error" + ex.Message);
        }
    }

    /// <summary>
    /// Converts an IEnumerable of Data Object (DO) Couriers into an IEnumerable of Business Object (BO) CourierInList summaries.
    /// This extension method iterates over the collection of DAL couriers and fetches the full, summarized BO list view for each one.
    /// It utilizes the 'GetCourierInList' helper function for the conversion and throws an exception if any courier ID is unexpectedly missing during the process.
    /// </summary>
    internal static IEnumerable<CourierInList> ToBoCourierInList(this IEnumerable<DO.Courier> dalCouriers)
    {
        try 
        {
            return dalCouriers.Select(courier => GetCourierInList(courier.Id) ?? throw new BO.BlDoesNotExistException($"Courier with ID={courier.Id} does Not exist"));
        }
        catch (BO.BlDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error converting DAL couriers to BO CourierInList." + ex.Message);
        }
    }

    /// <summary>
    /// Authenticates a user (manager or courier) based on the provided ID and password.
    /// It first attempts to verify the credentials against the hashed manager password stored in the system configuration.
    /// If the user is not the manager, it checks the provided ID against the courier database and verifies the courier's hashed password, returning "Manager" or "Courier" on success, or throwing an exception otherwise.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    /// <exception cref="BO.BlInvalidInputException"></exception>
    internal static string EnterSystem(int id, string password)
    {
        lock (AdminManager.BlMutex)
        {
            if (id == s_dal.Config.ManagerId)
            {
                if (Tools.ManageStrongPassword.VerifyPassword(password, s_dal.Config.ManagerPassword))
                    return "Manager";
                else
                    throw new BO.BlInvalidInputException("Invalid Password.");
            }
            else
            {
                DO.Courier doCourier = s_dal.Courier.Read(id) ?? throw new BO.BlDoesNotExistException($"Courier with ID={id} does Not exist");
                if (id == doCourier.Id)
                {
                    if (Tools.ManageStrongPassword.VerifyPassword(password, doCourier.PasswordCourier))
                        return "Courier";
                    else
                        throw new BO.BlInvalidInputException("Invalid Password.");
                }
            }
            throw new BO.BlInvalidInputException($"User with ID={id} does Not exist in the system.");
        }
    }
    /// <summary>
    /// Calculates the total monthly salary for an active courier, comprising base pay, distance component, and various bonuses.
    /// The calculation is based on the current system clock and includes bonuses for delivery volume, overall on-time performance, and seniority.
    /// If the courier is not active, the function returns null, and it throws an exception if the data layer is temporarily unavailable during the calculation process.
    /// </summary>
    /// <param name="courier"></param>
    /// <returns ></returns>
    internal static SalaryForCourier? CalculateCourierSalary(BO.Courier courier)
    {
        if (!courier.IsActive)
        {
            return null;
        }

        // Lock needed for reading DAL data (Config, Delivery)
        lock (AdminManager.BlMutex)
        {
            // Get current year and month from Now (which accesses Config clock)
            DateTime now = AdminManager.Now;
            int currentYear = now.Year;
            int currentMonth = now.Month;
            int courierId = courier.Id;

            double baseSalary = s_dal.Config.BaseSalaryMounthly;
            double deliveryVolumeBonus = 0;
            double performanceBonus = 0;
            double seniorityBonus = 0;
            // Get all deliveries for this courier in the current month that were supplied
            var deliveriesSupplid = s_dal.Delivery.ReadAll(d =>
                d.CourierId == courierId &&
                d.DeliveryTypeEnding == DO.DeliveryCompletionType.Supplied &&
                d.OrderEndDateTime.HasValue &&
                d.OrderEndDateTime.Value.Year == currentYear &&
                d.OrderEndDateTime.Value.Month == currentMonth
                ).ToList();

            double ratePerKm = s_dal.Config.DeliveryRatePerKm;
            double distanceSalaryComponent = deliveriesSupplid.Sum(d => d.DeliveryDistanceKm * ratePerKm) ?? 0.0;
            // Calculate Delivery Volume Bonus
            if (deliveriesSupplid.Count > 20)
            {
                deliveryVolumeBonus = (deliveriesSupplid.Count - 20) * 10.0;
            }

            double totalDeliveriesEver = courier.TotalDeliveredOnTime + courier.TotalLateDeliveries;

            if (totalDeliveriesEver > 0)
            {
                double onTimeRate = courier.TotalDeliveredOnTime / totalDeliveriesEver;

                if (onTimeRate >= 0.95)
                {
                    performanceBonus = baseSalary * 0.10;
                }
                else if (onTimeRate >= 0.85)
                {
                    performanceBonus = baseSalary * 0.05;
                }
            }

            TimeSpan workDuration = now - courier.StartDate;
            int yearsOfService = (int)(workDuration.TotalDays / 365.25);

            if (yearsOfService >= 5)
            {
                seniorityBonus = 500.0;
            }
            else if (yearsOfService >= 2)
            {
                seniorityBonus = 200.0;
            }

            double totalBonus = deliveryVolumeBonus + performanceBonus + seniorityBonus;
            double totalSalary = baseSalary + distanceSalaryComponent + totalBonus;

            return new SalaryForCourier()
            {
                Month = (Months)currentMonth,
                Year = currentYear,
                BaseSalary = baseSalary,
                DistanceSalaryComponent = distanceSalaryComponent,
                DeliveryVolumeBonus = deliveryVolumeBonus,
                PerformanceBonus = performanceBonus,
                SeniorityBonus = seniorityBonus,
                TotalSalary = totalSalary
            };
        }
    }

    /// <summary>
    /// Simulates the activity of couriers, including handling deliveries and selecting new orders.
    /// </summary>
    /// <remarks>This method performs a simulation of courier activities, including checking for ongoing
    /// deliveries, selecting new orders, and finalizing deliveries based on predefined probabilities and conditions.
    /// The simulation ensures thread safety by using locks where necessary and avoids overlapping runs through a mutex
    /// mechanism. Notifications are triggered to update observers after significant state changes.</remarks>
    /// <returns></returns>
    internal static async Task SimulateCourierActivityAsync() // Defined as internal static async Task per instructions
    {
        // Step 1: Mutex Check - Exit immediately if the previous run is still in progress
        if (s_simulationMutex.CheckAndSetInProgress())
            return;

        try
        {
            List<DO.Courier> activeCouriers;
            int idManager;

            // Step 2: DAL Read within lock - Retrieve Manager ID
            lock (AdminManager.BlMutex)
            {
                idManager = s_dal.Config.ManagerId;
            }
            // Step 2: DAL Read within lock - Retrieve active couriers
            // Use .ToList() immediately to create a concrete list and release the lock early
            lock (AdminManager.BlMutex)
            {
                activeCouriers = s_dal.Courier.ReadAll(c => c.Active).ToList();
            }

            foreach (var courier in activeCouriers)
            {
                DO.Delivery? currentDelivery;

                // Step 3: Check for an ongoing delivery for this courier
                lock (AdminManager.BlMutex)
                {
                    currentDelivery = s_dal.Delivery.ReadAll(d => d.CourierId == courier.Id && d.DeliveryTypeEnding == null)
                                                   .FirstOrDefault();
                }

                // CASE A: Courier has NO active delivery
                if (currentDelivery == null)
                {
                    // Probability of 0.15 that the courier is "available" to check for orders
                    if (s_rand.NextDouble() < 0.45) //changed to 45% for more activity
                    {
                        // Call BL method (potentially including network requests) outside of lock
                        var availableOrders = (await OrderManager.GetAvailableOrdersList(courier.Id, courier.Id,null,null)).ToList();

                        // If orders exist, 50% chance to actually select one
                        if (availableOrders.Any() && s_rand.NextDouble() < 0.5)
                        {
                            var randomOrder = availableOrders[s_rand.Next(availableOrders.Count)];

                            // Execute selection logic (this method handles its own DAL locks)
                            await OrderManager.SelectOrderForHandling(courier.Id, courier.Id, randomOrder.OrderId);

                            //// NOTIFICATIONS: Always called outside of lock blocks - not necessary to call here
                            //Observers.NotifyItemUpdated(courier.Id);
                            //OrderManager.Observers.NotifyItemUpdated(randomOrder.OrderId);
                        }
                    }
                }
                // CASE B: Courier HAS an active delivery
                else
                {
                    DateTime now;
                    DO.Order currentOrder;
                    lock (AdminManager.BlMutex)
                    {
                        now = AdminManager.Now;
                    }
                    lock (AdminManager.BlMutex)
                    {
                        currentOrder = s_dal.Order.Read(currentDelivery.OrderId) ?? throw new BO.BlDoesNotExistException($"Order with ID={currentDelivery.OrderId} does Not exist");
                    }

                    // Logic to determine if "enough time" has passed based on current simulation clock
                    double deliveryDuration = (now - currentDelivery.OrderStartDateTime).TotalDays;
                    DateTime? expectedDeliveryDate = await OrderManager.GetExpectedDeliveryTime(currentOrder);
                    if (expectedDeliveryDate.HasValue)
                    {
                        //calculate estimated required time + 2 days buffer
                        double estimatedRequiredTime = (expectedDeliveryDate.Value - currentDelivery.OrderStartDateTime).TotalDays + 2;
                        if (deliveryDuration > estimatedRequiredTime)
                        {
                            // Finalize delivery with random completion types for variety
                            double chance = s_rand.NextDouble();

                            DO.DeliveryCompletionType completionType = chance switch
                            {
                                < 0.75 => DO.DeliveryCompletionType.Supplied,
                                < 0.85 => DO.DeliveryCompletionType.CustomerRefused,
                                < 0.95 => DO.DeliveryCompletionType.CustomerNotFound,
                                _ => DO.DeliveryCompletionType.Failed
                            };

                            OrderManager.FinishDeliveryHandling(courier.Id, courier.Id, currentDelivery.Id, completionType);

                            // NOTIFICATIONS: Called after DAL operations are finished and lock is released -not necessary to call here
                            //Observers.NotifyItemUpdated(courier.Id);
                            //OrderManager.Observers.NotifyItemUpdated(currentDelivery.OrderId);
                        }
                    }
                    else
                    {
                        // 10% chance that a manager "cancels" the handling during the process
                        if (s_rand.NextDouble() < 0.1)
                        {
                            await OrderManager.CancelOrder(currentDelivery.OrderId, idManager);

                            
                        }
                    }
                }
            }
        }
        catch (BO.BlDoesNotExistException ex)
        {
            // Entity might have been deleted by another user. Log it and continue to next runner tick.
            System.Diagnostics.Debug.WriteLine($"Sim Warning: Entity not found. {ex.Message}");
        }
        catch (BO.BlNullPropertyException ex)
        {
            // Missing mandatory data. Throw to notify developer of data integrity issues.
            System.Diagnostics.Debug.WriteLine($"Sim Data Error: {ex.Message}");
            throw;
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            // External distance API failed. Log and continue (next second might succeed).
            System.Diagnostics.Debug.WriteLine($"Sim Network Error: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // Collection modified or logic error. Critical for simulation reliability.
            System.Diagnostics.Debug.WriteLine($"Sim Logic Error: {ex.Message}");
            throw;
        }
        catch (BlExternalServiceException ex)
        {
            throw new BlExternalServiceException("Error " + ex.Message);
        }
        catch (BlInvalidInputException ex)
        {
            throw new BlInvalidInputException("Error " + ex.Message);
        }
        finally
        {
            // Step 4: Release Mutex - Crucial to do this in 'finally' to avoid deadlocks on error
            s_simulationMutex.UnsetInProgress();
        }
    }

}

