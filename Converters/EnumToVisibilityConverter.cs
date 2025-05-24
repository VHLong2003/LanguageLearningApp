using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string enumValue = value.ToString();
            string targetValue = parameter.ToString();
            bool invert = targetValue.StartsWith("!");
            if (invert)
            {
                targetValue = targetValue.Substring(1);
            }

            bool isMatch = enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
            return invert ? !isMatch : isMatch;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}