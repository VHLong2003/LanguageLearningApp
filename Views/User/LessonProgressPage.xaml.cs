using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    public partial class LessonProgressPage : ContentPage
    {
        private LessonProgressViewModel _viewModel;

        public LessonProgressPage(LessonProgressViewModel viewModel)
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
    }
}
