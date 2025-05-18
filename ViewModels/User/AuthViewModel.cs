using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Services;
using System;
using System.Threading.Tasks;

namespace LanguageLearningApp.ViewModels.User
{
    public partial class AuthViewModel : ObservableObject
    {
        private readonly AuthService _authService = new AuthService();
        private readonly UserService _userService = new UserService();

        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private string fullName;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string errorMessage;

        public event EventHandler AuthSuccess;
        public event EventHandler AdminAuthSuccess;
        public event EventHandler RegisterSuccess;

        [RelayCommand]
        public async Task Login()
        {
            IsBusy = true;
            ErrorMessage = "";
            try
            {
                var response = await _authService.LoginAsync(Email, Password);
                var user = await _userService.GetUserByIdAsync(response.LocalId, response.IdToken);
                if (user == null)
                {
                    ErrorMessage = "Không tìm thấy thông tin người dùng!";
                    return;
                }

                // Lưu đăng nhập
                AuthStorage.SaveLogin(response.LocalId, response.IdToken, user.Role);

                if (user.Role.ToLower() == "admin")
                    AdminAuthSuccess?.Invoke(this, EventArgs.Empty);
                else
                    AuthSuccess?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task Register()
        {
            IsBusy = true;
            ErrorMessage = "";
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu nhập lại không khớp.";
                IsBusy = false;
                return;
            }
            try
            {
                var response = await _authService.RegisterAsync(Email, Password);

                var user = new Models.AppUser
                {
                    UserId = response.LocalId,
                    Email = Email,
                    FullName = FullName,
                    Role = "user",
                    Points = 0
                };
                await _userService.SaveUserAsync(user, response.IdToken);

                // Lưu đăng nhập ngay sau khi đăng ký thành công (auto login)
                AuthStorage.SaveLogin(response.LocalId, response.IdToken, "user");

                RegisterSuccess?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void Logout()
        {
            AuthStorage.ClearLogin();
        }
    }
}
