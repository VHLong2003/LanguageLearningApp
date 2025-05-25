using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearningApp.Converters
{
    public class StatusToIconConverter : IValueConverter
    {
        // Return the path to an icon based on status
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool completed)
                return completed ? "icon_lesson_completed.png" : "icon_lesson.png";
            return "icon_lesson.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
