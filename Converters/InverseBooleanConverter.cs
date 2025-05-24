using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle null value
            if (value == null)
            {
                return parameter?.ToString() == "IsNullOrEmpty" ? true : false;
            }

            // Handle string parameter for IsNullOrEmpty check
            if (parameter?.ToString() == "IsNullOrEmpty" && value is string str)
            {
                return string.IsNullOrEmpty(str);
            }

            // Handle IsTrue parameter for boolean inversion
            if (parameter?.ToString() == "IsTrue" && value is bool boolValue)
            {
                return !boolValue;
            }

            // Handle NotEqual parameter with a specific value
            if (parameter?.ToString().StartsWith("NotEqual") == true && value != null)
            {
                var parts = parameter.ToString().Split('|');
                if (parts.Length > 1 && value.ToString() == parts[1])
                {
                    return false;
                }
                return true;
            }

            // Default boolean inversion
            if (value is bool boolVal)
            {
                return !boolVal;
            }

            // Return false for unexpected types
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle null value
            if (value == null)
            {
                return parameter?.ToString() == "IsNullOrEmpty" ? true : false;
            }

            // Handle string parameter for IsNullOrEmpty check
            if (parameter?.ToString() == "IsNullOrEmpty" && value is string str)
            {
                return string.IsNullOrEmpty(str);
            }

            // Handle IsTrue parameter for boolean inversion
            if (parameter?.ToString() == "IsTrue" && value is bool boolValue)
            {
                return !boolValue;
            }

            // Handle NotEqual parameter with a specific value
            if (parameter?.ToString().StartsWith("NotEqual") == true && value != null)
            {
                var parts = parameter.ToString().Split('|');
                if (parts.Length > 1 && value.ToString() == parts[1])
                {
                    return false;
                }
                return true;
            }

            // Default boolean inversion
            if (value is bool boolVal)
            {
                return !boolVal;
            }

            // Return false for unexpected types
            return false;
        }
    }
}