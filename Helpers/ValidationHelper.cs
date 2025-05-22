using System;
using System.Text.RegularExpressions;

namespace LanguageLearningApp.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Simple regex check for email format
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string password)
        {
            // Check if password is at least 6 characters
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }

        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 2;
        }

        public static bool IsPositiveNumber(int number)
        {
            return number >= 0;
        }

        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true; // Empty URL is valid

            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
