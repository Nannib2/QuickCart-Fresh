using BO;
using DalApi;
using System;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Http;
using System.Collections.Concurrent;
using System.Linq.Expressions;
namespace Helpers;

internal static class Tools
{
    /// <summary>
    /// Provides a formatted string representation of all public properties and their values for any object (T), primarily for debugging or logging.
    /// This method uses reflection to iterate over properties, applying specific security and exclusion rules.
    /// </summary>
    /// <typeparam name="T">The type of the object to be converted to a string.</typeparam>
    /// <param name="t">The instance of the object.</param>
    /// <param name="propertyToSaveUnseen">Optional name of a property to explicitly exclude from the output.</param>
    /// <returns>A formatted string listing the name and value of each non-excluded property.</returns>
    internal static string ToStringProperty<T>(this T t, string? propertyToSaveUnseen = null)
    {
        string proprtiesStr = "";
        foreach (PropertyInfo item in typeof(T).GetProperties())
        {
            if (propertyToSaveUnseen != null && item.Name == propertyToSaveUnseen)
            {
                continue; // Skip this property
            }
            // Skip specific properties, only for security reasons
            if (item.Name == "Latitude" ||
                item.Name == "Longitude" ||
                item.Name == "PasswordCourier" ||
                item.Name == "ManagerPassword")
            {
                continue; // Skip this property
            }

            var value = item.GetValue(t, null);
            proprtiesStr += item.Name + ": ";

            if (value != null && value.GetType() != typeof(string) && value is IEnumerable)
            {
                proprtiesStr += "\n";
                foreach (var innerProperty in (IEnumerable<object>)value)
                {
                    proprtiesStr += innerProperty.ToString() + '\n';
                }
            }
            else
            {
                if (value == null)
                    proprtiesStr += "this property does not contain any value \n";
                else
                    proprtiesStr += value?.ToString() + '\n';
            }
        }
        return proprtiesStr;
    }


    ///Using AI helper
    ///https://gemini.google.com/share/f0a62df4aa71
    ///https://chatgpt.com/share/692c91a6-a0a8-800d-aab0-239cf670a5e5
    /// <summary>
    /// Provides a set of static methods for managing strong user passwords, including hashing,
    /// verification using salted PBKDF2, generation of cryptographically secure random passwords,
    /// and validation of password strength based on common security criteria.
    /// </summary>
    internal class ManageStrongPassword
    {

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

        /// <summary>
        /// Verifies a plain-text password against a stored salted and hashed password string.
        /// It extracts the salt from the stored hash, re-hashes the provided password with that salt,
        /// and performs a constant-time comparison (via the loop) against the stored hash value to prevent timing attacks.
        /// </summary>
        /// <param name="password">The plain-text password entered by the user.</param>
        /// <param name="storedHash">The stored Base64 string containing the salt and hash.</param>
        /// <returns>True if the password is correct, false otherwise.</returns>
        internal static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[16];
            const int SaltLength = 16;

            const int HashLength = 32;
            const int ExpectedLength = SaltLength + HashLength;


            if (hashBytes.Length < ExpectedLength)
            {

                throw new ArgumentException("Stored hash value is too short and invalid.", nameof(storedHash));
            }
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                }
            }

            return true;
        }



        // Define character sets
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DigitChars = "0123456789";
        private const string SpecialChars = "!@#$%^&*()_-+=[]{}\\|:;\"'<>,.?/";

        // Combine all character sets for random selection
        private const string AllChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;

        /// <summary>
        /// Generates a strong, random password of a specified length.
        /// It ensures the password includes at least one lowercase, one uppercase, one digit, and one special character
        /// by forcing these characters first, then filling the remaining length randomly, and finally shuffling the result.
        /// </summary>
        /// <param name="length">The desired length of the password (must be at least 8).</param>
        /// <returns>A randomly generated password string.</returns>
        /// <exception cref="BlInvalidInputException">Thrown if the desired password length is less than 8.</exception>
        internal string GeneratePassword(int length)
        {
            if (length < 8)
            {
                throw new BlInvalidInputException("Password length should be at least 8 for security.");
            }

            // 1. Ensure the password meets the minimum strength requirements
            // We force at least one of each required character type to be in the password.

            var requiredChars = new char[]
            {
            GetRandomChar(LowercaseChars),
            GetRandomChar(UppercaseChars),
            GetRandomChar(DigitChars),
            GetRandomChar(SpecialChars)
            };

            // 2. Generate the remaining characters randomly
            int remainingLength = length - requiredChars.Length;
            var passwordBuilder = new StringBuilder();

            // Append the minimum required characters first
            foreach (var c in requiredChars)
            {
                passwordBuilder.Append(c);
            }

            // Fill the rest of the password length with random characters from the combined set
            for (int i = 0; i < remainingLength; i++)
            {
                passwordBuilder.Append(GetRandomChar(AllChars));
            }

            // 3. Shuffle the password string to randomize the placement of the required characters
            // This prevents the password from always starting with the same four characters.

            return new string(passwordBuilder.ToString().OrderBy(x => GetRandomInt(0, 1000)).ToArray());
        }

        /// <summary>
        /// Gets a cryptographically secure random integer within a specified range [min, max).
        /// It uses the RandomNumberGenerator class to generate a secure random byte array,
        /// which is then converted to an integer and mapped to the desired range.
        /// </summary>
        /// <param name="min">The minimum value (inclusive).</param>
        /// <param name="max">The maximum value (exclusive).</param>
        /// <returns>A cryptographically secure random integer.</returns>
        internal int GetRandomInt(int min, int max)
        {
            // Use RNGCryptoServiceProvider for secure random number generation
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                rng.GetBytes(data);
                int generatedInt = BitConverter.ToInt32(data, 0);

                // Map the random integer to the desired range [min, max)
                return Math.Abs(generatedInt % (max - min)) + min;
            }
        }

        /// <summary>
        /// Selects a cryptographically secure random character from a given set of characters.
        /// It uses GetRandomInt to determine a random index within the character set string.
        /// </summary>
        /// <param name="charSet">The string containing the characters from which to select.</param>
        /// <returns>A single randomly selected character from the set.</returns>
        internal char GetRandomChar(string charSet)
        {
            int index = GetRandomInt(0, charSet.Length);
            return charSet[index];
        }

        ///Using AI helper
        ///https://gemini.google.com/share/45378af40bde
        /// <summary>
        /// Validates the strength of a password based on common security criteria.
        /// The criteria require a minimum length of 8 and at least one character from each of the following groups:
        /// uppercase letter, lowercase letter, digit, and special character.
        /// </summary>
        /// <param name="password">The password string to validate.</param>
        /// <returns>True if the password meets all strength criteria, false otherwise.</returns>
        internal static bool IsPasswordStrong(string password)
        {
            // 1. Check for null or empty password
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            // --- Define Minimum Requirements ---
            const int MIN_LENGTH = 8;

            // --- Password Checks ---

            // 2. Minimum Length Check
            if (password.Length < MIN_LENGTH)
            {
                // You can add logging or specific error messages here if needed
                return false;
            }

            // 3. Check for at least one uppercase letter (A-Z)
            // Regex: (?=.*[A-Z]) looks ahead for any character, zero or more times, followed by an uppercase letter.
            if (!Regex.IsMatch(password, @"(?=.*[A-Z])"))
            {
                return false;
            }

            // 4. Check for at least one lowercase letter (a-z)
            // Regex: (?=.*[a-z]) looks ahead for any character, zero or more times, followed by a lowercase letter.
            if (!Regex.IsMatch(password, @"(?=.*[a-z])"))
            {
                return false;
            }

            // 5. Check for at least one digit (0-9)
            // Regex: (?=.*\d) looks ahead for any character, zero or more times, followed by a digit.
            if (!Regex.IsMatch(password, @"(?=.*\d)"))
            {
                return false;
            }

            // 6. Check for at least one special character (non-alphanumeric/non-whitespace)
            // Regex: (?=.*[^\w\s]) looks ahead for a character that is NOT a word character or whitespace.
            if (!Regex.IsMatch(password, @"(?=.*[^\w\s])"))
            {
                return false;
            }

            // If all checks pass, the password is considered strong
            return true;
        }
    }

    /// <summary>
    /// Calculates the actual road distance (driving or walking) between two geographical points using the OSRM routing service.
    /// It selects the OSRM routing profile ("driving" or "foot") based on the courier's delivery type.
    /// </summary>
    /// <param name="lat1">Latitude of the starting point.</param>
    /// <param name="lon1">Longitude of the starting point.</param>
    /// <param name="lat2">Latitude of the destination point (nullable, but checked).</param>
    /// <param name="lon2">Longitude of the destination point (nullable, but checked).</param>
    /// <param name="deliveryType">The courier's mode of transport, determining the routing profile.</param>
    /// <returns>The actual distance in kilometers.</returns>
    /// <exception cref="BlNullPropertyException">Thrown if destination coordinates are null.</exception>
    /// Using AI assistance
    /// https://chatgpt.com/share/692c5ec5-0cf4-800d-9cd7-49c637654f59
    private static readonly ConcurrentDictionary<string, double> distanceCache = new();//A dictionary to store the distance values ​​calculated using a unique key.
                                                                                       // async type B                                                                                   //To prevent recalculation 
    internal static async Task<double> CalculateDistance(double lat1, double lon1, double? lat2, double? lon2, BO.DeliveryTypeMethods deliveryType)
    {
        try
        {
            if (lat2 == null || lon2 == null)
                throw new BlNullPropertyException("Config coordinates is missing.");

            string profile = deliveryType switch
            {
                BO.DeliveryTypeMethods.Motorcycle => "driving",
                BO.DeliveryTypeMethods.Car => "driving",
                BO.DeliveryTypeMethods.Bike => "foot",
                BO.DeliveryTypeMethods.Foot => "foot",
                _ => "driving"
            };
            string key = $"{lat1},{lon1}-{lat2},{lon2}-{profile}";//unique key
            if (distanceCache.TryGetValue(key, out double distance))
            {
                return distance;
            }

            double result = await OsrmDistanceKM(lat1, lon1, lat2.Value, lon2.Value, profile);
            distanceCache.TryAdd(key, result);

            return result;
        }
        catch (BlExternalServiceException ex)
        {
            throw new BlExternalServiceException("Error calculating distance: " + ex.Message);
        }
        catch(BlInvalidInputException ex)
        {
            throw new BlInvalidInputException("Error calculating distance: " + ex.Message);
        }
    }
    

  

    /// <summary>
    /// Calculates the driving or walking distance between two coordinates
    /// using the public OSRM routing service (no API key required).
    /// The method sends a request to the OSRM server with the selected profile
    /// (driving or foot), parses the returned JSON response, extracts the 
    /// route distance (in meters), and converts it to kilometers.
    /// </summary>
    /// <param name="lat1">Latitude of the starting point</param>
    /// <param name="lon1">Longitude of the starting point</param>
    /// <param name="lat2">Latitude of the destination point</param>
    /// <param name="lon2">Longitude of the destination point</param>
    /// <param name="profile">Profile for OSRM: "driving" or "foot"</param>
    /// <returns>The distance in kilometers as a double</returns>
    /// <exception cref="BlInvalidInputException">Thrown if OSRM cannot find any route between the points</exception
    /// Using AI assistance
    ///https://chatgpt.com/share/692c6a14-70b8-800d-bda5-374d91adba2a
   private static readonly HttpClient client = new HttpClient();//reuse HttpClient instance

    static Tools() // Constructor סטטי שרץ פעם אחת בלבד
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    }
    internal static async Task<double> OsrmDistanceKM(double lat1, double lon1, double lat2, double lon2, string profile)
    {
        // convert coordinates to invariant culture strings to avoid locale issues
        string lat1Str = lat1.ToString(CultureInfo.InvariantCulture);
        string lon1Str = lon1.ToString(CultureInfo.InvariantCulture);
        string lat2Str = lat2.ToString(CultureInfo.InvariantCulture);
        string lon2Str = lon2.ToString(CultureInfo.InvariantCulture);

       // client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"); //set User-Agent header, some servers reject requests without it

        // build OSRM API URL
        string url = $"http://router.project-osrm.org/route/v1/{profile}/" +
                     $"{lon1Str},{lat1Str};{lon2Str},{lat2Str}?overview=false";

        try
        {
            //send request to OSRM server
            //string json = client.GetStringAsync(url).GetAwaiter().GetResult();
            string json = await client.GetStringAsync(url); //await the async call

            //parse JSON response without dynamic
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            //check for "code" property to see if the request was successful
            if (root.TryGetProperty("code", out JsonElement codeElement) && codeElement.GetString() != "Ok")
            {
                throw new Exception($"OSRM Server Error: {codeElement.GetString()}");
            }

            //check that "routes" exists and contains data
            if (!root.TryGetProperty("routes", out JsonElement routes) || routes.GetArrayLength() == 0)
            {
                throw new BlInvalidInputException("No route found between the locations.");
            }

            //take first route and get distance
            JsonElement firstRoute = routes[0];

            if (!firstRoute.TryGetProperty("distance", out JsonElement distanceElement))
            {
                throw new BlInvalidInputException("Distance field missing in OSRM response.");
            }

            double meters = distanceElement.GetDouble();

            //convert meters to kilometers
            return meters / 1000.0;
        }
        catch (HttpRequestException ex)
        {
            return OrderManager.CalculateAirDistanceKm(lat1, lon1, lat2, lon2 );//as fallback use air distanceinorder that the prorgram will be able to countinue working
            //handle network errors
            throw new BlExternalServiceException("Network error while connecting to OSRM check your internet conection", ex);
        }
        catch (TaskCanceledException ex)
        {
            
            throw new BlExternalServiceException("Timeout: Could not retrieve coordinates from external service.", ex);
        }
    }
    /// <summary>
    /// fetches a secret value (e.g., API key) from a local "secrets.json" file based on the provided key name.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static string GetSecret(string key)
    {
        try
        {
            string path = "secrets.json";
            if (!System.IO.File.Exists(path)) return "";

            string json = System.IO.File.ReadAllText(path);
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(key, out JsonElement element))
            {
                return element.GetString() ?? "";
            }
        }
        catch { }
        return "";
    }

    /// <summary>
    /// Converts a given street address string into its corresponding geographic coordinates (Latitude and Longitude)
    /// by querying the OpenCage Geocoding API.
    /// </summary>
    /// <param name="address">The full street address string to geocode.</param>
    /// <returns>A double array containing {Latitude, Longitude}.</returns>
    /// <exception cref="BlInvalidInputException">Thrown if the address cannot be found or geocoding fails.</exception>
    /// Using AI assistance
    /// https://chatgpt.com/share/692c6a14-70b8-800d-bda5-374d91adba2a
    internal static async Task<double[]> CalculateCoordinate(string address)
    {
        string OPEN_CAGE_KEY = GetSecret("OpenCageApiKey");
        if (string.IsNullOrEmpty(OPEN_CAGE_KEY)) throw new Exception("API Key is missing from secrets.json");

        if (string.IsNullOrEmpty(OPEN_CAGE_KEY))
        {
            throw new Exception("OpenCage API Key is missing! Please set the OPENCAGE_API_KEY environment variable.");
        }

        using HttpClient client = new HttpClient();

        string url =
            $"https://api.opencagedata.com/geocode/v1/json?q={Uri.EscapeDataString(address)}&key={OPEN_CAGE_KEY}&language=en";

        var response = await client.GetAsync(url); //await the async call
        //var response = client.GetAsync(url).Result;
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(); //await the async call
        //string json = response.Content.ReadAsStringAsync().Result;

        var data = System.Text.Json.JsonSerializer.Deserialize<OpenCageResponse>(json);

        if (data == null || data.results == null || data.results.Count == 0)
            throw new BlInvalidInputException("Address not found.");

        double lat = data.results[0].geometry.lat;
        double lon = data.results[0].geometry.lng;

        return new double[] { lat, lon };
    }

    internal class OpenCageResponse
    {
        public List<OpenCageResult>? results { get; set; }
    }

    /// <summary>
    /// Class to model the root structure of the OpenCage Geocoding API response.
    /// </summary>
    internal class OpenCageResult
    {
        public OpenCageGeometry geometry { get; set; }
    }

    /// <summary>
    /// Class to model an individual result object within the OpenCage Geocoding API response.
    /// </summary>
    internal class OpenCageGeometry
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }


    /// <summary>
    /// Validates if a given string conforms to a specific local mobile phone number format:
    /// it must be exactly 10 characters long, start with the digit '0', and contain only digits.
    /// This implementation uses basic string checks and a loop, avoiding regular expressions for simplicity and potentially faster execution.
    /// </summary>
    /// <param name="phoneNumber">The phone number string to validate.</param>
    /// <returns>True if the number is valid according to the criteria, false otherwise.</returns>
    /// using AI assistance
    /// https://chatgpt.com/share/692c6a14-70b8-800d-bda5-374d91adba2a
    internal static bool IsValidMobileNumber(string phoneNumber)
    {
        // 1. Check for null or empty string
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return false;
        }

        // 2. Check if the string has exactly 10 characters
        if (phoneNumber.Length != 10)
        {
            return false;
        }

        // 3. Check if the string begins with the digit '0'
        if (phoneNumber[0] != '0')
        {
            return false;
        }

        // 4. Check if all other characters are digits using a simple loop
        for (int i = 1; i < phoneNumber.Length; i++)
        {
            if (!char.IsDigit(phoneNumber[i]))
            {
                return false;
            }
        }

        // If all checks pass, the number is valid
        return true;
    }

    /// <summary>
    /// Provides core email sending functionality for the business logic layer,
    /// utilizing the SMTP protocol to communicate with an external email provider (e.g., Gmail).
    /// This service is used for notifications to couriers and customers.
    /// </summary>
    internal class EmailService
{
    // Configure these settings for your email provider (e.g., Gmail, SendGrid, etc.)
    private const string SmtpHost = "smtp.gmail.com"; // e.g., "smtp.gmail.com"
    private const int SmtpPort = 587; // or 465 for SMTPS
        private static readonly string SmtpUsername = GetSecret("SmtpUsername");
        private static readonly string SmtpPassword = GetSecret("SmtpPassword");

        /// <summary>
        /// Sends an email message using the configured SMTP server and credentials.
        /// Supports a single primary recipient (To) and an optional list of blind carbon copy (BCC) recipients.
        /// Handles both HTML and plain text body formats.
        /// </summary>
        /// <param name="recipientEmail">The email address of the primary recipient.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="body">The HTML or plain text body of the email.</param>
        /// <param name="isHtml">True if the body is HTML, false for plain text.</param>
        /// <param name="bccList">Optional list of BCC addresses (e.g., for all couriers).</param>
        /// <exception cref="BlExternalServiceException">Thrown if the email sending process fails due to connection or authentication issues.</exception>
        /// usuing AI assistance
        /// https://gemini.google.com/share/1ec93a13e74f
        internal static async Task SendEmail(string recipientEmail, string subject, string body, bool isHtml = true, List<string>? bccList = null)
    {
        try
        {
            // 1. Create the Mail Message
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(SmtpUsername, "QuickCart Fresh");
                mail.To.Add(recipientEmail);

                if (bccList != null)
                {
                    foreach (string bcc in bccList)
                    {
                        mail.Bcc.Add(bcc);
                    }
                }

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = isHtml;

                // 2. Configure the SMTP Client
                using (SmtpClient smtp = new SmtpClient(SmtpHost, SmtpPort))
                {
                    smtp.Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
                    smtp.EnableSsl = true; // Use SSL/TLS

                    // 3. Send the Email
                    //smtp.Send(mail);
                    await smtp.SendMailAsync(mail); //await the async call
                        Console.WriteLine($"Email successfully sent to {recipientEmail}.");
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            throw new BlExternalServiceException($"Error sending email to {recipientEmail}: {ex.Message}");
            // In a real application, you might re-throw the exception or use a proper logging framework.
        }
    }
}
    ///AI assistance
    ///https://gemini.google.com/share/1ec93a13e74f
    /// <summary>
    /// Notifies a list of couriers about a new delivery order available for acceptance.
    /// </summary>
    /// <remarks>Sends an email to all specified couriers using BCC to ensure privacy of email addresses. The
    /// email contains the order ID and detailed pickup/delivery information.</remarks>
    /// <param name="order">The order details (DO.Order), including the order ID and pickup location.</param>
    /// <param name="courierEmails">A list of email addresses of couriers to be notified.</param>
    /// <param name="companyAddress">The address of the company (pickup location).</param>
    internal static async Task NotifyCouriersOfNewOrder(DO.Order order, List<string> courierEmails,string? companyAddress)
    {
        string subject = $" NEW ORDER AVAILABLE: #{order.Id}";

        // Example of a simple HTML body
        string body = $@"
        <p>A new delivery order is available near you!</p>
        <p><strong>Order ID:</strong> {order.Id}</p>
        <p><strong>Pickup Location:</strong> {companyAddress}</p>
        <p><strong>order Location:</strong> {order.OrderAddress}</p>
        <p><strong>Pickup Short order description:</strong> {order.ShortOrderDescription}</p>
        <p><strong>Pickup Amount of items:</strong> {order.AmountItems}</p>
        <p><strong>Pickup customer full name:</strong> {order.CustomerFullName}</p>
        <p><strong>Pickup Customer phone:</strong> {order.CustomerPhone}</p>
        <p><strong>Pickup order type:</strong> {order.OrderType}</p>

    ";

        // You can send a single email with all couriers in the BCC list
        // The "recipientEmail" here can be a dummy address or the first courier's email.
        // The BCC ensures that all couriers get the email, but none see the others' addresses.
        await EmailService.SendEmail(
             recipientEmail: GetSecret("AdminEmail"),
             subject: subject,
             body: body,
             bccList: courierEmails
         );
    }

    ///AI assistance
    ///https://gemini.google.com/share/1ec93a13e74f
    /// <summary>
    /// Sends a confirmation email to the courier who accepted the order, including detailed order and delivery information.
    /// </summary>
    /// <param name="order">The order details (BO.Order) that was accepted.</param>
    /// <param name="courier">The courier details (DO.Courier) who accepted the order.</param>
    /// <param name="companyAddress">The address of the company (pickup location).</param>
    internal static async Task ConfirmOrderToCourier(BO.Order order, DO.Courier courier, string? companyAddress) 
    {
       
        string subject = $" ORDER ACCEPTED: #{order.Id}";

        // Detailed body for the handling courier
        string body = $@"
        <p>Thank you for accepting order <strong>#{order.Id}</strong>. Here are the details:</p>
        <ul>
        <p><strong>Pickup Location:</strong> {companyAddress}</p>
        <p><strong>order Location:</strong> {order.FullAddress}</p>
        <p><strong>Air Distance:</strong> {order.AirDistance}</p>
        <p><strong>Short order description:</strong> {order.ShortOrderDescription}</p>
        <p><strong>Amount of items:</strong> {order.AmountItems}</p>
        <p><strong>customer full name:</strong> {order.CustomerFullName}</p>
        <p><strong>Customer phone:</strong> {order.CustomerPhone}</p>
        <p><strong>order type:</strong> {order.OrderType}</p>
        <p><strong>Maximum Delivery Time:</strong> {order.MaximumDeliveryTime}</p>
        </ul>
        <p>Start your delivery now!</p>
    ";

       await  EmailService.SendEmail(
            recipientEmail: courier.EmailCourier, // Only the handling courier gets this email
            subject: subject,
            body: body
        );
    }

    ///AI assistance
    ///https://gemini.google.com/share/1ec93a13e74f
    /// <summary>
    /// Sends a cancellation notification email to the courier who was assigned the now-cancelled order.
    /// </summary>
    /// <param name="order">The cancelled order details (BO.Order).</param>
    /// <param name="courier">The courier details (DO.Courier) who was assigned the order.</param>
    internal static async Task NotifyCourierOfCancellation(BO.Order order, DO.Courier courier)
    {
        string subject = $" ORDER CANCELLED: #{order.Id}";

        string body = $@"
        <p>Attention {courier.NameCourier},</p>
        <p>Order <strong>#{order.Id}</strong>, which you were handling, has been CANCELLED.</p>
        <p>Please discontinue delivery and follow the standard return procedure for the items.</p>
        <p>If you have any questions, please contact dispatch.</p>
    ";

       await EmailService.SendEmail(
            recipientEmail: courier.EmailCourier,
            subject: subject,
            body: body
        );
    }
    /// <summary>
    /// Validates an Israeli ID number using the standard checksum algorithm.
    /// The algorithm requires summing weighted digits and checking if the result is divisible by 10.
    /// </summary>
    /// <param name="id">The Israeli ID number as an integer.</param>
    /// <returns>True if the ID is valid, false otherwise.</returns>
    /// using AI assistance
    /// https://gemini.google.com/share/9a13cd237ad1
    internal static bool IsValidIsraeliId(int id)
    {
        if (id <= 0) return false;
        // 1. Convert the ID to a 9-digit zero-padded string.
        // The maximum value for an int is 2,147,483,647 (10 digits).
        // A 9-digit ID will be max 999,999,999, which fits easily.
        string idString = id.ToString().PadLeft(9, '0');

        // Check if the padded string is indeed 9 digits long.
        // This check is mainly for string conversion edge cases, but good practice.
        if (idString.Length != 9)
        {
            return false;
        }

        // 2. Apply the standard checksum algorithm.
        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            // Get the digit at the current position.
            // i starts at 0 (leftmost digit) and goes up to 8 (checksum digit).
            int digit = idString[i] - '0'; // Convert char to int

            // Determine the weight: 1 for odd positions (1st, 3rd, 5th, etc.), 2 for even positions (2nd, 4th, 6th, etc.)
            // Since array indexing starts at 0:
            // Index 0 (1st digit) -> weight 1
            // Index 1 (2nd digit) -> weight 2
            // Index 2 (3rd digit) -> weight 1
            int weightedDigit = (i % 2 == 0) ? digit * 1 : digit * 2;

            // Add the weighted digit to the sum.
            // IMPORTANT: If the weighted digit is a two-digit number (10-18), 
            // the two digits must be summed (e.g., 12 -> 1 + 2 = 3).
            sum += (weightedDigit > 9) ? (weightedDigit - 9) : weightedDigit;
        }

        // 3. The ID is valid if the final sum is divisible by 10.
        return sum % 10 == 0;
    }
    /// <summary>
    /// sends a notification email to the courier about a change in delivery time.
    /// </summary>
    /// <param name="doDelivery"></param>
    /// <param name="doCourier"></param>
    /// <param name="message"></param>
    /// 
    internal static async Task NotifyCouriersOfDeliveryTimeChange(DO.Delivery doDelivery,DO.Courier doCourier ,string message)
    {
        string subject = $" Pay attention time changed.:";
        string body = $@"
        <p><strong></strong> {message}</p>
        <p><strong> you started to deliver at:</strong> {doDelivery.OrderStartDateTime}</p>
        <p><strong>Pickup Short order description:</strong> {doDelivery.DeliveryType}</p>
        <p>Please hurry</p>
    ";

        // You can send a single email with all couriers in the BCC list
        // The "recipientEmail" here can be a dummy address or the first courier's email.
        // The BCC ensures that all couriers get the email, but none see the others' addresses.
       await  EmailService.SendEmail(
            recipientEmail: doCourier.EmailCourier,
            subject: subject,
            body: body
            
        );
    }

}






