using Microsoft.Maui.Storage;

namespace LanguageLearningApp.Views.Admin
{
    public partial class AdminDashboardPage : ContentPage
    {
        public AdminDashboardPage()
        {
            InitializeComponent();
        }

        private async void ManageUsers_Clicked(object sender, EventArgs e)
        {
            var idToken = Preferences.Get("IdToken", null);
            await Navigation.PushAsync(new UsersManagementPage());
        }



        private async void ManageCourses_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CoursesManagementPage());
        }
    }
}
