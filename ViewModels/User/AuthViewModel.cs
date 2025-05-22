using System;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.User
{
    public class AuthViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;

        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _fullName;
        private string _errorMessage;
        private string _successMessage;
        private bool _isLoading;
        private string _forgotEmail;

        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
        public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
        public string SuccessMessage { get => _successMessage; set => SetProperty(ref _successMessage, value); }
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
        public string ForgotEmail { get => _forgotEmail; set => SetProperty(ref _forgotEmail, value); }

        public ICommand RegisterCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand GoToLoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }
        public ICommand GoToForgotPasswordCommand { get; }

        public AuthViewModel(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;

            RegisterCommand = new Command(async () => await RegisterAsync());
            LoginCommand = new Command(async () => await LoginAsync());
            ForgotPasswordCommand = new Command(async () => await ForgotPasswordAsync());
            GoToLoginCommand = new Command(async () => await GoToLoginAsync());
            GoToRegisterCommand = new Command(async () => await GoToRegisterAsync());
            GoToForgotPasswordCommand = new Command(async () => await GoToForgotPasswordAsync());
        }

        // ĐĂNG KÝ (GỬI MAIL XÁC MINH)
        private async Task RegisterAsync()
        {
            ErrorMessage = SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Vui lòng điền đầy đủ thông tin";
                return;
            }
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu không khớp";
                return;
            }
            if (!ValidationHelper.IsValidEmail(Email))
            {
                ErrorMessage = "Email không hợp lệ";
                return;
            }
            if (!ValidationHelper.IsValidPassword(Password))
            {
                ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự";
                return;
            }

            IsLoading = true;
            try
            {
                var (success, message, userId, token) = await _authService.RegisterAsync(Email, Password, FullName);

                if (success)
                {
                    // Gửi mail xác minh
                    await _authService.SendVerificationEmailAsync(token);
                    SuccessMessage = "Đăng ký thành công. Vui lòng kiểm tra email và xác minh trước khi đăng nhập!";
                }
                else
                {
                    ErrorMessage = message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đăng ký lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ĐĂNG NHẬP (CHỈ CHO ĐĂNG NHẬP NẾU ĐÃ XÁC MINH EMAIL)
        private async Task LoginAsync()
        {
            ErrorMessage = SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập email và mật khẩu";
                return;
            }

            IsLoading = true;
            try
            {
                var (success, message, userId, token) = await _authService.LoginAsync(Email, Password);

                if (success)
                {
                    // Kiểm tra xác minh email
                    var isVerified = await _authService.IsEmailVerifiedAsync(token);
                    if (!isVerified)
                    {
                        ErrorMessage = "Bạn cần xác minh email trước khi đăng nhập. Vui lòng kiểm tra hộp thư và nhấn vào liên kết xác minh.";
                        return;
                    }

                    var user = await _userService.GetUserByIdAsync(userId, token);
                    LocalStorageHelper.SetItem("userId", userId);
                    LocalStorageHelper.SetItem("idToken", token);
                    LocalStorageHelper.SetItem("userRole", user.Role);

                    if (user.Role == "Admin")
                        await Shell.Current.GoToAsync("///admin");
                    else
                        await Shell.Current.GoToAsync("///user");
                }
                else
                {
                    ErrorMessage = message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đăng nhập lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // QUÊN MẬT KHẨU (GỬI MAIL RESET)
        private async Task ForgotPasswordAsync()
        {
            ErrorMessage = SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ForgotEmail))
            {
                ErrorMessage = "Vui lòng nhập email";
                return;
            }

            if (!ValidationHelper.IsValidEmail(ForgotEmail))
            {
                ErrorMessage = "Email không hợp lệ";
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _authService.ResetPasswordAsync(ForgotEmail);
                if (result)
                {
                    SuccessMessage = "Đã gửi email khôi phục mật khẩu. Vui lòng kiểm tra hộp thư!";
                }
                else
                {
                    ErrorMessage = "Gửi mail thất bại. Thử lại sau.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Gửi mail thất bại: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Navigation
        private async Task GoToLoginAsync() => await Shell.Current.GoToAsync("login");
        private async Task GoToRegisterAsync() => await Shell.Current.GoToAsync("register");
        private async Task GoToForgotPasswordAsync() => await Shell.Current.GoToAsync("forgotPassword");
    }
}
