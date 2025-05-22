using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LanguageLearningApp.Converters
{
    public class RankToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rank)
            {
                // Gold, Silver, Bronze for top 3
                switch (rank)
                {
                    case 1:
                        return Color.FromArgb("#FFD700"); // Gold
                    case 2:
                        return Color.FromArgb("#C0C0C0"); // Silver
                    case 3:
                        return Color.FromArgb("#CD7F32"); // Bronze
                    default:
                        return Color.FromArgb("#3498db"); // Regular blue color for all other ranks
                }
            }

            return Colors.Gray; // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
