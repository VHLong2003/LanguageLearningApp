using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class EnumEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // If parameter has a ConverterParameter, use that instead
            if (parameter is IMarkupExtension markupExt &&
                markupExt.GetType().GetProperty("ConverterParameter")?.GetValue(markupExt) is object convParam)
            {
                parameter = convParam;
            }

            // Handle case for parameter being a bool (for inverted checks)
            if (parameter is bool boolParam)
            {
                bool isEqual = value.ToString() == parameter.ToString();
                return boolParam ? isEqual : !isEqual;
            }

            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
