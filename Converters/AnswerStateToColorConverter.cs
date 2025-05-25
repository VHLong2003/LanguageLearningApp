using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class AnswerStateToColorConverter : IValueConverter
    {
        // ConverterParameter truyền vào ThisPage để lấy SelectedAnswer từ BindingContext
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string answer = value?.ToString();
            var page = parameter as ContentPage;
            if (page?.BindingContext is ViewModels.User.LessonViewModel vm)
            {
                if (vm.HasAnswered)
                {
                    if (answer == vm.SelectedAnswer)
                        return vm.IsAnswerCorrect ? Color.FromArgb("#22c55e") : Color.FromArgb("#ef4444");
                    // Nếu đáp án đúng nhưng không được chọn
                    if (answer == vm.CurrentQuestion?.CorrectAnswer)
                        return Color.FromArgb("#bbf7d0");
                    return Color.FromArgb("#f3f4f6");
                }
                else
                {
                    if (answer == vm.SelectedAnswer)
                        return Color.FromArgb("#dbeafe");
                    return Color.FromArgb("#f3f4f6");
                }
            }
            return Color.FromArgb("#f3f4f6");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
