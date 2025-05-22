using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    public partial class CoursesPage : ContentPage
    {
        private readonly CoursesViewModel _viewModel;

        public CoursesPage(CourseService courseService, UserService userService, ProgressService progressService)
        {
            InitializeComponent();
            _viewModel = new CoursesViewModel(courseService, userService, progressService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
