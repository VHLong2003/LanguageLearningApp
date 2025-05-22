using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class NullToColorConverter : IValueConverter
    {
        // parameter = "colorWhenNull,colorWhenNotNull"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colors = parameter?.ToString()?.Split(',');
            if (colors == null || colors.Length < 2)
                return "#E0E7FF";
            return value == null ? colors[0] : colors[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
