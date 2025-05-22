using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class ImageUrlToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                // If parameter is provided, use it as a default image
                if (parameter != null && !string.IsNullOrEmpty(parameter.ToString()))
                {
                    return ImageSource.FromFile(parameter.ToString());
                }

                // Return null or a default image
                return null;
            }

            string imageUrl = value.ToString();

            // Check if it's a remote URL or a local resource
            if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // It's a remote URL
                return ImageSource.FromUri(new Uri(imageUrl));
            }
            else
            {
                // It's a local resource
                return ImageSource.FromFile(imageUrl);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed for this converter
            throw new NotImplementedException();
        }
    }
}
