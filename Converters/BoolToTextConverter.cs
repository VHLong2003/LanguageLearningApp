using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearningApp.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        // ConverterParameter="TextIfTrue|TextIfFalse"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = parameter as string;
            var texts = param?.Split('|');
            if (value is bool b && texts?.Length == 2)
                return b ? texts[0] : texts[1];
            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
