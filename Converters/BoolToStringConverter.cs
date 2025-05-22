using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                if (parameter is string options)
                {
                    var parts = options.Split('|');
                    if (parts.Length == 2)
                    {
                        return isTrue ? parts[0] : parts[1];
                    }
                }

                return isTrue ? "Yes" : "No";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
