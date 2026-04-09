using DalApi;

namespace Dal
{
    /// <summary>
    /// Provides an implementation of the <see cref="IConfig"/> interface for the Data Access Layer (DAL).
    /// This class serves as a wrapper around the static <see cref="Config"/> class,
    /// allowing configuration values to be accessed and modified via an instance-based interface.
    /// </summary>
    internal class ConfigImplementation : IConfig
    {
        /// <summary>
        /// Gets or sets the current system clock time used by the company.
        /// <para>This property simply delegates the call to the static <see cref="Config.Clock"/> property.</para>
        /// </summary>
        public DateTime Clock
        {
            get => Config.Clock;
            set => Config.Clock = value;
        }

        /// <summary>
        /// Gets or sets the manager's unique ID.
        /// <para>This property simply delegates the call to the static <see cref="Config.ManagerId"/> property.</para>
        /// </summary>
        public int ManagerId
        {
            get => Config.ManagerId;
            set => Config.ManagerId = value;
        }

        /// <summary>
        /// Gets or sets the manager's password.
        /// <para>This property simply delegates the call to the static <see cref="Config.ManagerPassword"/> property.</para>
        /// </summary>
        public string ManagerPassword
        {
            get => Config.ManagerPassword;
            set => Config.ManagerPassword = value;
        }

        /// <summary>
        /// Gets or sets the company's main address.
        /// <para>This property simply delegates the call to the static <see cref="Config.AddressCompany"/> property.</para>
        /// </summary>
        public string? AddressCompany
        {
            get => Config.AddressCompany;
            set => Config.AddressCompany = value;
        }

        /// <summary>
        /// Gets or sets the latitude coordinate of the company's location.
        /// <para>This property simply delegates the call to the static <see cref="Config.Latitude"/> property.</para>
        /// </summary>
        public double? Latitude
        {
            get => Config.Latitude;
            set => Config.Latitude = value;
        }

        /// <summary>
        /// Gets or sets the longitude coordinate of the company's location.
        /// <para>This property simply delegates the call to the static <see cref="Config.Longitude"/> property.</para>
        /// </summary>
        public double? Longitude
        {
            get => Config.Longitude;
            set => Config.Longitude = value;
        }

        /// <summary>
        /// Gets or sets the maximum air delivery distance allowed (in kilometers).
        /// <para>This property simply delegates the call to the static <see cref="Config.MaxAirDeliveryDistanceKm"/> property.</para>
        /// </summary>
        public double? MaxAirDeliveryDistanceKm
        {
            get => Config.MaxAirDeliveryDistanceKm;
            set => Config.MaxAirDeliveryDistanceKm = value;
        }

        /// <summary>
        /// Gets or sets the average motorcycle delivery speed (in km/h).
        /// <para>This property simply delegates the call to the static <see cref="Config.AverageMotorcycleSpeedKmh"/> property.</para>
        /// </summary>
        public double AverageMotorcycleSpeedKmh
        {
            get => Config.AverageMotorcycleSpeedKmh;
            set => Config.AverageMotorcycleSpeedKmh = value;
        }

        /// <summary>
        /// Gets or sets the average bicycle delivery speed (in km/h).
        /// <para>This property simply delegates the call to the static <see cref="Config.AverageBicycleSpeedKmh"/> property.</para>
        /// </summary>
        public double AverageBicycleSpeedKmh
        {
            get => Config.AverageBicycleSpeedKmh;
            set => Config.AverageBicycleSpeedKmh = value;
        }

        /// <summary>
        /// Gets or sets the average car delivery speed (in km/h).
        /// <para>This property simply delegates the call to the static <see cref="Config.AverageCarSpeedKmh"/> property.</para>
        /// </summary>
        public double AverageCarSpeedKmh
        {
            get => Config.AverageCarSpeedKmh;
            set => Config.AverageCarSpeedKmh = value;
        }

        /// <summary>
        /// Gets or sets the average walking delivery speed (in km/h).
        /// <para>This property simply delegates the call to the static <see cref="Config.AverageWalkingSpeedKmh"/> property.</para>
        /// </summary>
        public double AverageWalkingSpeedKmh
        {
            get => Config.AverageWalkingSpeedKmh;
            set => Config.AverageWalkingSpeedKmh = value;
        }

        /// <summary>
        /// Gets or sets the maximum delivery time range allowed.
        /// <para>This property simply delegates the call to the static <see cref="Config.MaxDeliveryTimeRange"/> property.</para>
        /// </summary>
        public TimeSpan MaxDeliveryTimeRange
        {
            get => Config.MaxDeliveryTimeRange;
            set => Config.MaxDeliveryTimeRange = value;
        }

        /// <summary>
        /// Gets or sets the time range that defines when a delivery is considered at risk.
        /// <para>This property simply delegates the call to the static <see cref="Config.RiskTimeRange"/> property.</para>
        /// </summary>
        public TimeSpan RiskTimeRange
        {
            get => Config.RiskTimeRange;
            set => Config.RiskTimeRange = value;
        }

        /// <summary>
        /// Gets or sets the inactivity time range, after which a courier or delivery is considered inactive.
        /// <para>This property simply delegates the call to the static <see cref="Config.InactivityTimeRange"/> property.</para>
        /// </summary>
        public TimeSpan InactivityTimeRange
        {
            get => Config.InactivityTimeRange;
            set => Config.InactivityTimeRange = value;
        }

        /// <summary>
        ///get and set base salary mounthly
        /// </summary>
        public double BaseSalaryMounthly
        {
            get => Config.BaseSalaryMounthly;
            set => Config.BaseSalaryMounthly = value;
        }
        /// <summary>
        /// get and set delivery rate per km
        /// </summary>
        public double DeliveryRatePerKm
        {
            get => Config.DeliveryRatePerKm;
            set => Config.DeliveryRatePerKm = value;
        }

        /// <summary>
        /// Resets all configuration settings to their default values.
        /// <para>This method delegates the call to the static <see cref="Config.Reset"/> method.</para>
        /// </summary>
        public void Reset()
        {
            Config.Reset();
        }
    }
}