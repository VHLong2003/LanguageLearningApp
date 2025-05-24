using System;
using System.Globalization;
using LanguageLearningApp.Models;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class MultipleChoiceVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is QuestionType type && type == QuestionType.MultipleChoice;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
