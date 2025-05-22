using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class BoolToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                if (parameter is string doubleString)
                {
                    var parts = doubleString.Split('|');
                    if (parts.Length == 2 &&
                        double.TryParse(parts[0], out double trueValue) &&
                        double.TryParse(parts[1], out double falseValue))
                    {
                        return isTrue ? trueValue : falseValue;
                    }
                }

                return isTrue ? 1.0 : 0.5;
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
