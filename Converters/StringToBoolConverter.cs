using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            string stringValue = value.ToString();

            // Check if it's actually evaluating against a specific string
            if (parameter != null)
            {
                return stringValue == parameter.ToString();
            }

            // Otherwise just check if the string is not empty
            return !string.IsNullOrWhiteSpace(stringValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not typically used in this direction
            throw new NotImplementedException();
        }
    }
}
