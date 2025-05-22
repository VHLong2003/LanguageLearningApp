using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Converters
{
    public class CategorySelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return "#E0E7FF"; // Màu mặc định

            var selected = value.ToString();
            var current = parameter.ToString();
            // Nếu đang chọn, trả về màu nổi bật
            return selected == current ? "#6366f1" : "#E0E7FF";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
