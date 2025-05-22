using System;
using System.Globalization;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Converters
{
    public class LessonIdToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string lessonId)
            {
                try
                {
                    // Get the LessonService from DI
                    var lessonService = App.Current.Handler.MauiContext.Services.GetService<LessonService>();
                    var idToken = LocalStorageHelper.GetItem("idToken");

                    // Get lesson title asynchronously (not ideal in a converter but works for simple cases)
                    var task = lessonService.GetLessonByIdAsync(lessonId, idToken);
                    task.Wait();

                    var lesson = task.Result;
                    if (lesson != null)
                    {
                        return lesson.Title;
                    }
                }
                catch
                {
                    // Fallback if we can't get the title
                }

                return $"Lesson {lessonId}";
            }

            return "Unknown Lesson";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
