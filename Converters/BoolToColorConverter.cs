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
                        var trueColor = ParseColor(parts[0]);
                        var falseColor = ParseColor(parts[1]);
                        return isTrue ? trueColor : falseColor;
                    }
                }
                return isTrue ? Colors.Green : Colors.Red;
            }
            return Colors.Gray;
        }

        private Color ParseColor(string colorString)
        {
            colorString = colorString.Trim();

            // Cho phép mã hex (bắt đầu bằng # hoặc không), và tên màu chuẩn.
            if (colorString.StartsWith("#"))
                return Color.FromArgb(colorString);

            // Xử lý nếu truyền vào là tên màu chuẩn (ví dụ "Red")
            var colorProp = typeof(Colors).GetProperty(colorString, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
            if (colorProp != null)
                return (Color)colorProp.GetValue(null);

            // Nếu là mã hex nhưng không có dấu #
            if (colorString.Length == 6 || colorString.Length == 8)
                return Color.FromArgb("#" + colorString);

            // Nếu bị lỗi, trả về Gray
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
