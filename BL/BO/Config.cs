
using Helpers;

namespace BO;

public class Config
{
    /// <summary>
    /// Gets or sets the current time represented by the clock.
    /// </summary>
    public DateTime Clock { get; set; }
    /// <summary>
    ///  manager ID for authentication
    /// </summary>
    public int ManagerId { get; set; }
    /// <summary>
    ///     manager password for authentication
    /// </summary>
    public string ManagerPassword { get; set; }
    /// <summary>
    /// address company name
    /// </summary>
    public string? AddressCompany { get; set; }
    /// <summary>
    /// latitude of the company location
    /// </summary>
    public double? Latitude { get; set; }
    /// <summary>
    ///  longitude of the company location
    /// </summary>
    public double? Longitude { get; set; }
    /// <summary>
    ///   maximum air delivery distance in kilometers
    /// </summary>
    public double? MaxAirDeliveryDistanceKm { get; set; }
    /// <summary>
    /// average speed of different vehicle types in kilometers per hour
    /// <summary>
    public double AverageMotorcycleSpeedKmh { get; set; }
    public double AverageBicycleSpeedKmh { get; set; }
    public double AverageCarSpeedKmh { get; set; }
    public double AverageWalkingSpeedKmh { get; set; }
    /// <summary>
    /// maximum delivery time range
    /// </summary>
    public TimeSpan MaxDeliveryTimeRange { get; set; }
    /// <summary>
    /// risk time range for delivery personnel
    /// </summary>
    public TimeSpan RiskTimeRange { get; set; }
    /// <summary>
    /// inactivity time range for delivery personnel
    /// <summary>
    /// 
   public TimeSpan InactivityTimeRange { get; set; }
    /// <summary>
    /// resets the configuration to its default values
    /// </summary>

    /// <summary>
    /// base salary per month for employees.
    /// </summary>
    public double BaseSalaryMounthly { get; set; }

    /// <summary>
    /// delivery rate per kilometer.
    /// </summary>
    public double DeliveryRatePerKm { get; set; }

    public override string ToString() => Tools.ToStringProperty<Config>(this);
}
