using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using LanguageLearningApp.ViewModels;

namespace LanguageLearningApp.Converters
{
    public class AnswerToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Colors.Transparent;

            bool isSelected = value.ToString() == parameter.ToString();

            // We need to check if the view has an ancestor with HasAnswered property
            if (Application.Current.MainPage?.BindingContext is BaseViewModel viewModel)
            {
                var hasAnsweredProperty = viewModel.GetType().GetProperty("HasAnswered");
                if (hasAnsweredProperty != null && (bool)hasAnsweredProperty.GetValue(viewModel))
                {
                    // Check if this is the correct answer
                    var correctAnswerProperty = viewModel.GetType().GetProperty("CorrectAnswer");
                    if (correctAnswerProperty != null)
                    {
                        var correctAnswer = correctAnswerProperty.GetValue(viewModel)?.ToString();

                        if (parameter.ToString() == correctAnswer)
                        {
                            // This is the correct answer
                            return Colors.Green.WithAlpha(0.3f);
                        }
                        else if (isSelected)
                        {
                            // This is selected but wrong
                            return Colors.Red.WithAlpha(0.3f);
                        }
                    }
                }
            }

            // Default: just highlight the selected answer
            return isSelected ? Colors.LightBlue : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
