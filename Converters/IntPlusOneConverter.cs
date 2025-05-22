using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class IntPlusOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue + 1;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue - 1;
            }
            return value;
        }
    }
}
