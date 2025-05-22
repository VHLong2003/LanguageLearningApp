using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (parameter != null)
                {
                    // If parameter is provided, check equality
                    if (int.TryParse(parameter.ToString(), out int paramValue))
                    {
                        return intValue == paramValue;
                    }
                }

                // Otherwise, just check if it's not zero
                return intValue != 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
