
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Dal;
/// <summary>
/// Configuration class for managing global settings and identifiers.
/// </summary>
internal static class Config
{
    /// <summary>
    /// Starting point for order IDs.
    /// </summary>
    internal const int startOrderId = 1000;
    private static int nextOrderId = startOrderId;
    /// <summary>
    /// Gets the next available order ID and increments the counter.
    /// </summary>
    internal static int NextOrderId { [MethodImpl(MethodImplOptions.Synchronized)]  get => nextOrderId++; }
    /// <summary>
    /// Starting point for delivery IDs.
    /// </summary>
    internal const int startDeliveryId = 1000;
    private static int nextDeliveryId = startDeliveryId;
    /// <summary>
    /// Gets the next available delivery ID and increments the counter.
    /// </summary>
    internal static int NextDeliveryId { [MethodImpl(MethodImplOptions.Synchronized)]  get => nextDeliveryId++; }
    /// <summary>
    /// Global system clock used to represent the current simulated time.
    /// </summary>
    internal static DateTime Clock { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; } = new DateTime(2025, 9, 1, 0, 0, 0);
    /// <summary>
    /// Unique identifier for the system manager.
    /// </summary>
    internal static int ManagerId { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// Password assigned to the system manager.
    /// </summary>
    internal static string ManagerPassword { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }   

    /// <summary>
    /// The company's physical address.
    /// </summary>
    internal static string? AddressCompany { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; } = null;

    /// <summary>
    /// Latitude coordinate of the company location.
    /// </summary>
    internal static double? Latitude { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; } = null;

    /// <summary>
    /// Longitude coordinate of the company location.
    /// </summary>
    internal static double? Longitude { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; } = null;

    /// <summary>
    /// Maximum allowed air delivery distance in kilometers.
    /// </summary>
    internal static double? MaxAirDeliveryDistanceKm { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; } = null;

    /// <summary>
    /// Average motorcycle delivery speed in kilometers per hour.
    /// </summary>
    internal static double AverageMotorcycleSpeedKmh { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// Average bicycle delivery speed in kilometers per hour.
    /// </summary>
    internal static double AverageBicycleSpeedKmh { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// Average car delivery speed in kilometers per hour.
    /// </summary>
    internal static double AverageCarSpeedKmh { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// Average walking delivery speed in kilometers per hour.
    /// </summary>
    internal static double AverageWalkingSpeedKmh { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// Maximum allowed delivery time range.
    /// </summary>
    internal static TimeSpan MaxDeliveryTimeRange { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// Time range representing potential risk duration.
    /// </summary>
    internal static TimeSpan RiskTimeRange { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// Time range representing inactivity or idle period.
    /// </summary>
    internal static TimeSpan InactivityTimeRange { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// base salary per month for employees.
    /// </summary>
    internal static double BaseSalaryMounthly { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }
    /// <summary>
    /// delivery rate per kilometer.
    /// </summary>
    internal static double DeliveryRatePerKm { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

    /// <summary>
    /// Resets all configuration values and running identifiers to their initial defaults.
    /// </summary>
    /// <remarks>
    /// This method restores IDs, clears system parameters, and sets all time ranges to zero.
    /// </remarks>

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        // Reset running IDs
        nextOrderId = startOrderId;
        nextDeliveryId = startDeliveryId;

        // Reset system configuration values
        Clock = new DateTime(2025, 9, 1, 0, 0, 0);

        ManagerId = 123456782;
        ManagerPassword = HashPassword("Manager!2345#");
        AddressCompany = "Ben Yehuda 12, Jerusalem, Israel";
        Latitude = 31.781488;
        Longitude = 35.2175176;
        MaxAirDeliveryDistanceKm = 50;
        AverageMotorcycleSpeedKmh = 45;
        AverageBicycleSpeedKmh = 10;
        AverageCarSpeedKmh = 45;
        AverageWalkingSpeedKmh = 10;
        MaxDeliveryTimeRange = TimeSpan.FromDays(7);
        RiskTimeRange = TimeSpan.FromDays(2);
        InactivityTimeRange = TimeSpan.FromDays(60);
        BaseSalaryMounthly = 2500;
        DeliveryRatePerKm = 5;
    }

    /// <summary>
    /// Generates a cryptographically secure hash of a given password using PBKDF2 with SHA256 and a high iteration count (100,000).
    /// A unique, random 16-byte salt is generated for each password. The salt and the resulting 32-byte hash are combined
    /// and returned as a Base64-encoded string for secure storage.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A Base64 string containing the combined salt and hash.</returns>
    internal static string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the password with the salt using PBKDF2
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash
                                               // Combine salt + hash
            byte[] hashBytes = new byte[48]; // 16 bytes salt + 32 bytes hash
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);
            // Convert to base64 for storage
            return Convert.ToBase64String(hashBytes);
        }
    }
}



