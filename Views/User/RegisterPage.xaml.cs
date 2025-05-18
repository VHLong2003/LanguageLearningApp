using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    public partial class RegisterPage : ContentPage
    {
        private AuthViewModel ViewModel => BindingContext as AuthViewModel;

        public RegisterPage()
        {
            InitializeComponent();
            if (ViewModel != null)
            {
                ViewModel.RegisterSuccess += OnRegisterSuccess;
            }
        }

        private async void OnRegisterSuccess(object sender, EventArgs e)
        {
            // Đăng ký thành công chuyển về login
            await Navigation.PopAsync();
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
