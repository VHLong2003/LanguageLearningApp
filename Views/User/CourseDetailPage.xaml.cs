using LanguageLearningApp.ViewModels.User;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Views.User
{
    public partial class CourseDetailPage : ContentPage
    {
        private readonly CourseDetailViewModel _viewModel;

        public CourseDetailPage(CourseDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!string.IsNullOrEmpty(_viewModel.CourseId))
            {
                await _viewModel.InitializeAsync(_viewModel.CourseId);
            }
        }
    }
}
