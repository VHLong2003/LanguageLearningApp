using System.Globalization;

namespace LanguageLearningApp.Converters
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            // Trả về tên hiển thị đẹp hơn của enum
            return value.ToString().SplitCamelCase();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class StringExtensions
    {
        public static string SplitCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1").Trim();
        }
    }
}
