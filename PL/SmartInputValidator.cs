using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace PL
{
    /// <summary>
    /// Supported validation types for user input.
    /// </summary>
    public enum ValidationType
    {
        /// <summary>
        /// Checks if the field contains any text.
        /// </summary>
        NotEmpty,

        /// <summary>
        /// Checks if the input is a valid POSITIVE integer (whole number > 0).
        /// </summary>
        Integer,

        /// <summary>
        /// Checks if the input is a valid double (floating-point number).
        /// </summary>
        Double,

        /// <summary>
        /// Checks if the input is a valid TimeSpan format (e.g., "12:30", "1.05:00:00").
        /// </summary>
        TimeSpan,

        /// <summary>
        /// Checks if the input is a valid email address.
        /// </summary>
        Email,

        /// <summary>
        /// Checks if the input follows the address format: "Street Num, City, Country".
        /// </summary>
        Address
    }

    /// <summary>
    /// A custom ValidationRule class to handle multiple data types in XAML bindings.
    /// </summary>
    public class SmartInputValidator : ValidationRule
    {
        /// <summary>
        /// Gets or sets the type of validation to perform.
        /// </summary>
        public ValidationType Type { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (value ?? " ").ToString();

            // 1. Basic Check: Ensure the field is not empty or whitespace only.
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ValidationResult(false, "This field is required.");
            }

            // If we only need to check for non-empty input, return valid.
            if (Type == ValidationType.NotEmpty)
            {
                return ValidationResult.ValidResult;
            }

            // 2. Type-Specific Validation
            switch (Type)
            {
                case ValidationType.Integer:
                    // Check if input is a valid integer AND is positive (> 0)
                    if (!int.TryParse(input, NumberStyles.Integer, cultureInfo, out int intResult) || intResult <= 0)
                    {
                        return new ValidationResult(false, "Please enter a positive whole number (greater than 0).");
                    }
                    break;

                case ValidationType.Double:
                    if (!double.TryParse(input, NumberStyles.Any, cultureInfo, out _))
                    {
                        return new ValidationResult(false, "Please enter a valid number.");
                    }
                    break;

                case ValidationType.TimeSpan:
                    if (!TimeSpan.TryParse(input, cultureInfo, out _))
                    {
                        return new ValidationResult(false, "Please enter a valid time (e.g., 12:30 or 1.05:00).");
                    }
                    break;

                case ValidationType.Email:
                    if (!Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        return new ValidationResult(false, "Please enter a valid email address.");
                    }
                    break;

                case ValidationType.Address:
                    // Regex Explanation:
                    // ^                 -> Start of string
                    // [\w\s]+           -> Street name (Words/Spaces)
                    // \s                -> Space required before number
                    // \d+               -> Street Number (Digits)
                    // ,                 -> Comma required
                    // \s* -> Optional space
                    // [\w\s]+           -> City
                    // ,                 -> Comma required
                    // \s* -> Optional space
                    // [\w\s]+           -> Country
                    // $                 -> End of string
                    // Matches format: "Ben Yehuda 12, Jerusalem, Israel"
                    if (!Regex.IsMatch(input, @"^[\w\s\.-]+\s\d+,\s*[\w\s\.-]+,\s*[\w\s\.-]+$"))
                    {
                        return new ValidationResult(false, "Format: Street Number, City, Country (e.g., Ben Yehuda 12, Jerusalem, Israel)");
                    }
                    break;
            }

            // If logic passes, the input is valid.
            return ValidationResult.ValidResult;
        }
    }
}