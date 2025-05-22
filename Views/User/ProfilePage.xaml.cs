using LanguageLearningApp.ViewModels.User;
using System.Threading.Tasks;

namespace LanguageLearningApp.Views.User
{
    [QueryProperty(nameof(UserId), "userId")]
    public partial class ProfilePage : ContentPage
    {
        private ProfileViewModel _viewModel;
        private string _userId;

        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                LoadUserProfile();
            }
        }

        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserProfile();
        }

        private async void LoadUserProfile()
        {
            await _viewModel.InitializeAsync(_userId);
        }
    }
}
