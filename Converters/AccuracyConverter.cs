using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class AccuracyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int correct = 0, total = 0;
            if (value is int c) correct = c;

            // parameter truyền vào là tổng số câu
            if (parameter is int t) total = t;
            else if (parameter is string s && int.TryParse(s, out int st)) total = st;

            if (total == 0) return "0%";
            double percent = (double)correct / total * 100;
            return $"{percent:0.#}%";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
