using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;

namespace LanguageLearningApp.Converters
{
    public class NotCurrentUserConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var userId = value as string;
            var currentUserId = LocalStorageHelper.GetItem("userId");
            return userId != currentUserId;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
