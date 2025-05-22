using System;
using System.Collections;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class BoolToCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is IMarkupExtension markupExt)
            {
                // Format is expected to be {Binding Collection1}|{Binding Collection2}
                if (markupExt.GetType().GetProperty("ConverterParameter")?.GetValue(markupExt) is string paramString)
                {
                    var parts = paramString.Split('|');
                    if (parts.Length == 2)
                    {
                        // The first part would be accessed through a binding to the true case
                        // The second part would be accessed through a binding to the false case
                        try
                        {
                            var bindingParts = parts[boolValue ? 0 : 1].Trim().Split('.');
                            if (bindingParts.Length == 2 && bindingParts[0] == "Binding")
                            {
                                // Extract property name from the binding expression
                                string propertyName = bindingParts[1];

                                // Get the binding context (assumed to be the same as for this converter)
                                var bindingContext = ((BindableObject)parameter).BindingContext;

                                // Get the property value using reflection
                                var property = bindingContext.GetType().GetProperty(propertyName);
                                if (property != null)
                                {
                                    return property.GetValue(bindingContext);
                                }
                            }
                        }
                        catch
                        {
                            // Fall back to default if parsing fails
                        }
                    }
                }
            }

            // If we couldn't parse the parameter or it's not in expected format
            return value is bool isTrue && isTrue
                ? parameter
                : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
