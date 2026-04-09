
namespace DalApi;

/// <summary>
/// defines the configuration settings for the delivery system
/// </summary>
public interface IConfig
{
    /// <summary>
    /// Gets or sets the current time represented by the clock.
    /// </summary>
    DateTime Clock { get; set; }
    /// <summary>
    ///  manager ID for authentication
    /// </summary>
    int ManagerId { get; set; }
    /// <summary>
    ///     manager password for authentication
    /// </summary>
    string ManagerPassword { get; set; }
    /// <summary>
    /// address company name
    /// </summary>
    string? AddressCompany { get; set; }
    /// <summary>
    /// latitude of the company location
    /// </summary>
    double? Latitude { get; set; }
    /// <summary>
    ///  longitude of the company location
    /// </summary>
    double? Longitude { get; set; }
    /// <summary>
    ///   maximum air delivery distance in kilometers
    /// </summary>
    double? MaxAirDeliveryDistanceKm { get; set; }
    /// <summary>
    /// average speed of different vehicle types in kilometers per hour
    /// <summary>
    double AverageMotorcycleSpeedKmh { get; set; }
    double AverageBicycleSpeedKmh { get; set; }
    double AverageCarSpeedKmh { get; set; }
    double AverageWalkingSpeedKmh { get; set; }
    /// <summary>
    /// maximum delivery time range
    /// </summary>
    TimeSpan MaxDeliveryTimeRange { get; set; }
    /// <summary>
    /// risk time range for delivery personnel
    /// </summary>
    TimeSpan RiskTimeRange { get; set; }

    /// <summary>
    /// inactivity time range for delivery personnel
    /// <summary>
    TimeSpan InactivityTimeRange { get; set; }

    /// <summary>
    /// base salary per month for employees.
    /// </summary>
    double BaseSalaryMounthly { get; set; }
    /// <summary>
    /// delivery rate per kilometer.
    /// </summary>
    double DeliveryRatePerKm { get; set; }
    /// <summary>
    /// resets the configuration to its default values
    /// </summary>
    void Reset();

}
