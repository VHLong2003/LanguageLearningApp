using LanguageLearningApp.Helpers;

namespace LanguageLearningApp.Views.User
{
    public partial class AccountPage : ContentPage
    {
        public AccountPage()
        {
            InitializeComponent();
        }

        private async void Logout_Clicked(object sender, EventArgs e)
        {
            AuthStorage.ClearLogin();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
