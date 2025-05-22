using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.Auth
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(AuthService authService, UserService userService )
        {
            InitializeComponent();
            BindingContext = new AuthViewModel(authService, userService);
        }
    }
}
