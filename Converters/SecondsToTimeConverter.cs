using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int seconds)
            {
                if (seconds < 60)
                {
                    return $"{seconds} sec";
                }
                else if (seconds < 3600)
                {
                    int minutes = seconds / 60;
                    int remainingSeconds = seconds % 60;
                    return $"{minutes}m {remainingSeconds}s";
                }
                else
                {
                    int hours = seconds / 3600;
                    int minutes = (seconds % 3600) / 60;
                    return $"{hours}h {minutes}m";
                }
            }

            return "0 sec";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
