using LanguageLearningApp.Models;
using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    public partial class HomeUserPage : ContentPage
    {
        private HomeViewModel _viewModel;

        public HomeUserPage(HomeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }

        private void OnCourseSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                var selectedCourse = (CourseModel)e.CurrentSelection[0];
                _viewModel.CourseSelectedCommand.Execute(selectedCourse);

                // Reset selection
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}
