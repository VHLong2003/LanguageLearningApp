using LanguageLearningApp.Helpers;

namespace LanguageLearningApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var userId = Helpers.AuthStorage.GetUserId();
            var idToken = Helpers.AuthStorage.GetIdToken();
            var role = Helpers.AuthStorage.GetRole();

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(idToken))
            {
                // Nếu là admin thì vào dashboard admin, ngược lại là user
                if (role?.ToLower() == "admin")
                    MainPage = new NavigationPage(new Views.Admin.AdminDashboardPage());
                else
                    MainPage = new Views.User.MainTabPage();
            }
            else
            {
                MainPage = new NavigationPage(new Views.User.LoginPage());
            }
        }
    }



}
