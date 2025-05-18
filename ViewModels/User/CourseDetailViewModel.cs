using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageLearningApp.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LanguageLearningApp.ViewModels.User
{
    public partial class CourseDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Course course;

        [RelayCommand]
        private async Task StartLesson(Lesson lesson)
        {
            // Chuyển qua màn học bài/quiz
            if (lesson != null)
            {
                await Shell.Current.GoToAsync($"LearningPage", new Dictionary<string, object>
                {
                    { "Lesson", lesson }
                });
            }
        }
    }
}
