using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    public partial class LeaderboardPage : ContentPage
    {
        private readonly LeaderboardViewModel _viewModel;

        public LeaderboardPage(LeaderboardViewModel viewModel)
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
