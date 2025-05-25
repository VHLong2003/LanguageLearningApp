using LanguageLearningApp.ViewModels.User;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Views.User
{
    public partial class CoursesPage : ContentPage
    {
        private readonly CoursesViewModel _viewModel;

        public CoursesPage(CoursesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel.Courses == null || _viewModel.Courses.Count == 0)
            {
                await _viewModel.InitializeAsync();
            }
        }

        
    }
}
