using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is provided, it indicates we should invert the logic
                if (parameter != null && parameter.ToString().ToLower() == "invert")
                {
                    boolValue = !boolValue;
                }

                return boolValue;
            }

            return true; // Default to visible
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool visibility)
            {
                if (parameter != null && parameter.ToString().ToLower() == "invert")
                {
                    return !visibility;
                }

                return visibility;
            }

            return false;
        }
    }
}
