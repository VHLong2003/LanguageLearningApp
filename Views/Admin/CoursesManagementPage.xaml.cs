using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.Admin;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Views.Admin
{
    public partial class CoursesManagementPage : ContentPage
    {
        public CoursesManagementPage()
        {
            InitializeComponent();
            BindingContext = new AdminCourseViewModel(
                App.Services.GetService<CourseService>(),
                App.Services.GetService<StorageService>()
            );
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AdminCourseViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
