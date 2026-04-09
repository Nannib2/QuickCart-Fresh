using DO; // Import the namespace containing custom exceptions
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dal;

/// <summary>
/// This static class holds all configuration settings, constants, and auto-incrementing IDs 
/// for the Data Access Layer (DAL). It acts as a wrapper around the XMLTools to read and write 
/// configuration values from the 'data-config' XML file, ensuring thread-safe access 
/// and proper exception handling.
/// </summary>
internal static class Config
{
    /// <summary>
    /// File name constant for the general configuration XML file.
    /// </summary>
    internal const string s_data_config_xml = "data-config.xml";
    /// <summary>
    /// File name constant for the couriers data XML file.
    /// </summary>
    internal const string s_couriers_xml = "couriers.xml";
    /// <summary>
    /// File name constant for the orders data XML file.
    /// </summary>
    internal const string s_orders_xml = "orders.xml";
    /// <summary>
    /// File name constant for the deliveries data XML file.
    /// </summary>
    internal const string s_deliverys_xml = "deliverys.xml";

    /// <summary>
    /// Gets the next available Order ID and automatically increments the value in the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or incrementing the value fails.</exception>
    internal static int NextOrderId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get and increase NextOrderId: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get and increase NextOrderId: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private set
        {
            try
            {
                XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set NextOrderId: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets the next available Delivery ID and automatically increments the value in the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or incrementing the value fails.</exception>
    internal static int NextDeliveryId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get and increase NextDeliveryId: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get and increase NextDeliveryId: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private set
        {
            try
            {
                XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set NextDeliveryId: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the Manager's ID from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static int ManagerId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigIntVal(s_data_config_xml, "ManagerId");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get ManagerId: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get ManagerId: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigIntVal(s_data_config_xml, "ManagerId", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set ManagerId: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the Manager's Password from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading or setting the value fails.</exception>
    internal static string ManagerPassword
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigStringVal(s_data_config_xml, "ManagerPassword").ToString();
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get ManagerPassword: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get ManagerPassword: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigStringVal(s_data_config_xml, "ManagerPassword", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set ManagerPassword: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the Company's address from/to the configuration file. Nullable string.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading or setting the value fails.</exception>
    internal static string? AddressCompany
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigNullableStringVal(s_data_config_xml, "AddressCompany");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AddressCompany: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AddressCompany: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {

                XMLTools.SetConfigStringVal(s_data_config_xml, "AddressCompany", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set AddressCompany: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the Company's Latitude from/to the configuration file. Nullable double.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static double? Latitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "Latitude");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get Latitude: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get Latitude: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "Latitude", value!);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set Latitude: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the Company's Longitude from/to the configuration file. Nullable double.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static double? Longitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "Longitude");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get Longitude: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get Longitude: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "Longitude", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set Longitude: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum air delivery distance in kilometers from/to the configuration file. Nullable double.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static double? MaxAirDeliveryDistanceKm
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "MaxAirDeliveryDistanceKm");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get MaxAirDeliveryDistanceKm: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get MaxAirDeliveryDistanceKm: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "MaxAirDeliveryDistanceKm", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set MaxAirDeliveryDistanceKm: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the average motorcycle speed in km/h from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static double AverageMotorcycleSpeedKmh
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageMotorcycleSpeedKmh");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AverageMotorcycleSpeedKmh: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AverageMotorcycleSpeedKmh: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageMotorcycleSpeedKmh", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set AverageMotorcycleSpeedKmh: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the average bicycle speed in km/h from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static double AverageBicycleSpeedKmh
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeedKmh");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AverageBicycleSpeedKmh: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AverageBicycleSpeedKmh: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeedKmh", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set AverageBicycleSpeedKmh: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the average car speed in km/h from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static double AverageCarSpeedKmh
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageCarSpeedKmh");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AverageCarSpeedKmh: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AverageCarSpeedKmh: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageCarSpeedKmh", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set AverageCarSpeedKmh: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the average walking speed in km/h from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static double AverageWalkingSpeedKmh
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageWalkingSpeedKmh");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AverageWalkingSpeedKmh: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get AverageWalkingSpeedKmh: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageWalkingSpeedKmh", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set AverageWalkingSpeedKmh: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum expected delivery time range (TimeSpan) from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static TimeSpan MaxDeliveryTimeRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTimeRange");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get MaxDeliveryTimeRange: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get MaxDeliveryTimeRange: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTimeRange", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set MaxDeliveryTimeRange: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the time range (TimeSpan) that classifies a delivery as 'at risk' from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static TimeSpan RiskTimeRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskTimeRange");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get RiskTimeRange: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get RiskTimeRange: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskTimeRange", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set RiskTimeRange: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the time range (TimeSpan) that classifies an entity as inactive from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static TimeSpan InactivityTimeRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "InactivityTimeRange");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get InactivityTimeRange: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get InactivityTimeRange: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "InactivityTimeRange", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set InactivityTimeRange: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets or sets the current simulation clock time (DateTime) from/to the configuration file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if reading, parsing, or setting the value fails.</exception>
    internal static DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                return XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get Clock: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get Clock: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set Clock: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// base salary per month for employees (global minimum).
    /// </summary>
    internal static double BaseSalaryMounthly
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            { 
                XElement root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
                double val = root.ToDoubleNullable("BaseSalaryMounthly")
                    ?? throw new FormatException($"Missing or unparsable value for BaseSalaryMounthly in {s_data_config_xml}");
                return val;
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get BaseSalaryMounthly: {ex.Message}");
            }
            catch (FormatException ex)
            { 
                throw new DalXMLFileLoadCreateException($"Failed to get BaseSalaryMounthly: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "BaseSalaryMounthly", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set BaseSalaryMounthly: {ex.Message}");
            }
        }
    }


    /// <summary>
    /// delivery rate per kilometer (global rate).
    /// </summary>
    internal static double DeliveryRatePerKm
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            try
            {
                XElement root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
                double val = root.ToDoubleNullable("DeliveryRatePerKm")
                    ?? throw new FormatException($"Missing or unparsable value for DeliveryRatePerKm in {s_data_config_xml}");
                return val;
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get DeliveryRatePerKm: {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to get DeliveryRatePerKm: {ex.Message}");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            try
            {
                XMLTools.SetConfigDoubleVal(s_data_config_xml, "DeliveryRatePerKm", value);
            }
            catch (DalXMLFileLoadCreateException ex)
            {
                throw new DalXMLFileLoadCreateException($"Failed to set DeliveryRatePerKm: {ex.Message}");
            }
        }
    }


    /// <summary>
    /// Resets all configuration values to their initial default state.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if writing any configuration value fails.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        // The individual setters handle the XML writing exceptions
        NextOrderId = 1000;
        NextDeliveryId = 1000;
        // Reset system configuration values
        Clock  = new DateTime(2025, 9, 1, 0, 0, 0);
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