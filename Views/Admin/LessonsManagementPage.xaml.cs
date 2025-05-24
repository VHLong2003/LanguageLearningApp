using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.Admin;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Views.Admin
{
    public partial class LessonsManagementPage : ContentPage
    {
        public LessonsManagementPage()
        {
            InitializeComponent();
            BindingContext = new AdminLessonViewModel(
                App.Services.GetService<LessonService>(),
                App.Services.GetService<CourseService>(),
                App.Services.GetService<StorageService>()
            );
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AdminLessonViewModel vm)
            {
                // Nếu CourseId đã truyền qua Route, sẽ tự động load dữ liệu
                if (!string.IsNullOrEmpty(vm.CourseId))
                    await vm.LoadCourseAndLessonsAsync();
            }
        }
    }
}
