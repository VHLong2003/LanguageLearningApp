using LanguageLearningApp.ViewModels.User;
using LanguageLearningApp.Views.Admin;

namespace LanguageLearningApp.Views.User
{
    public partial class LoginPage : ContentPage
    {
        private AuthViewModel ViewModel => BindingContext as AuthViewModel;

        public LoginPage()
        {
            InitializeComponent();
            if (ViewModel != null)
            {
                ViewModel.AuthSuccess += OnUserLoginSuccess;
                ViewModel.AdminAuthSuccess += OnAdminLoginSuccess;
            }
        }

        private async void OnUserLoginSuccess(object sender, EventArgs e)
        {
            Application.Current.MainPage = new Views.User.MainTabPage();
        }


        private void OnAdminLoginSuccess(object sender, EventArgs e)
        {
            Application.Current.MainPage = new NavigationPage(new AdminDashboardPage());
        }

        private async void RegisterButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }

}
