using System;
using System.Globalization;
using LanguageLearningApp.Models;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class QuestionTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QuestionType type && parameter is string param)
                return type.ToString() == param;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
