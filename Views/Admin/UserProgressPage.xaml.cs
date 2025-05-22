using LanguageLearningApp.ViewModels.Admin;

namespace LanguageLearningApp.Views.Admin
{
    [QueryProperty(nameof(UserId), "userId")]
    public partial class UserProgressPage : ContentPage
    {
        private UserProgressViewModel _viewModel;
        private string _userId;

        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                LoadUserData();
            }
        }

        public UserProgressPage(UserProgressViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private async void LoadUserData()
        {
            if (!string.IsNullOrEmpty(_userId))
            {
                await _viewModel.InitializeAsync(_userId);
            }
        }
    }
}
