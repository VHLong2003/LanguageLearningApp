using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class CategoryButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Color.FromArgb("#f3f4f6");
            return value.ToString() == parameter.ToString()
                ? Color.FromArgb("#e0e7ff") // màu xanh nhạt
                : Color.FromArgb("#f3f4f6"); // xám nhạt
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
