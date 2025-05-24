using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class QuestionTypeToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
                if (memberInfo != null)
                {
                    var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
                    return displayAttribute?.Name ?? enumValue.ToString();
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}