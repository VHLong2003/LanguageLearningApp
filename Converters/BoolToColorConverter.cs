using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LanguageLearningApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                if (parameter is string colors)
                {
                    var parts = colors.Split('|');
                    if (parts.Length == 2)
                    {
                        return isTrue ?
                            Color.FromArgb(parts[0]) :
                            Color.FromArgb(parts[1]);
                    }
                }

                return isTrue ? Colors.Green : Colors.Red;
            }

            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
