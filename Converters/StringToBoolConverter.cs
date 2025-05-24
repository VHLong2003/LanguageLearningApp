using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return !string.IsNullOrEmpty(stringValue);
            }
            if (value is QuestionType questionType && parameter is string param)
            {
                return questionType.ToString() == param;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}