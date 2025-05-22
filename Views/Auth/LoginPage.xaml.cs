using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.Auth
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthViewModel _viewModel;

        public LoginPage(AuthService authService, UserService userService)
        {
            InitializeComponent();
            _viewModel = new AuthViewModel(authService, userService);
            BindingContext = _viewModel;
        }
    }
}
