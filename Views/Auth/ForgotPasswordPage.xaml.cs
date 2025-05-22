using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.User;


namespace LanguageLearningApp.Views.Auth
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage(AuthService authService, UserService userService)
        {
            InitializeComponent();
            BindingContext = new AuthViewModel(authService, userService);
        }
    }

}
