using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;
        private readonly UserService _userService;

        public AuthService(FirebaseConfig firebaseConfig, UserService userService)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
            _userService = userService;
        }

        // Đăng ký tài khoản và gửi mail xác minh
        public async Task<(bool success, string message, string userId, string token)> RegisterAsync(string email, string password, string fullName)
        {
            try
            {
                var data = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_firebaseConfig.ApiKey}",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<FirebaseAuthResponse>(responseContent);

                    // Tạo profile user trong Realtime Database
                    var newUser = new UsersModel
                    {
                        UserId = result.LocalId,
                        Email = email,
                        FullName = fullName,
                        Role = "User",
                        Points = 0,
                        Coins = 100,
                        DateJoined = DateTime.Now,
                        LastActive = DateTime.Now,
                        CurrentStreak = 0,
                        AvatarUrl = "default_avatar.png"
                    };

                    await _userService.CreateUserAsync(newUser, result.IdToken);

                    // Gửi mail xác minh
                    await SendEmailVerificationAsync(result.IdToken);

                    return (true, "Đăng ký thành công. Vui lòng kiểm tra email để xác minh.", result.LocalId, result.IdToken);
                }
                else
                {
                    var error = JsonConvert.DeserializeObject<FirebaseAuthError>(responseContent);
                    return (false, error.Error.Message, null, null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Đăng ký thất bại: {ex.Message}", null, null);
            }
        }

        // Đăng nhập và trả về userId, token
        public async Task<(bool success, string message, string userId, string token)> LoginAsync(string email, string password)
        {
            try
            {
                var data = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseConfig.ApiKey}",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<FirebaseAuthResponse>(responseContent);
                    return (true, "Đăng nhập thành công", result.LocalId, result.IdToken);
                }
                else
                {
                    var error = JsonConvert.DeserializeObject<FirebaseAuthError>(responseContent);
                    return (false, error.Error.Message, null, null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Đăng nhập thất bại: {ex.Message}", null, null);
            }
        }

        // Gửi email xác minh
        public async Task<bool> SendEmailVerificationAsync(string idToken)
        {
            try
            {
                var data = new
                {
                    requestType = "VERIFY_EMAIL",
                    idToken = idToken
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_firebaseConfig.ApiKey}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Kiểm tra đã xác minh email chưa
        public async Task<bool> IsEmailVerifiedAsync(string idToken)
        {
            try
            {
                var data = new { idToken = idToken };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={_firebaseConfig.ApiKey}",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic obj = JsonConvert.DeserializeObject(responseContent);
                    return obj.users[0].emailVerified == true;
                }
            }
            catch { }
            return false;
        }
        // Gửi mail xác minh sau khi đăng ký thành công
        public async Task<bool> SendVerificationEmailAsync(string idToken)
        {
            try
            {
                var data = new
                {
                    requestType = "VERIFY_EMAIL",
                    idToken = idToken
                };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_firebaseConfig.ApiKey}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var data = new
                {
                    email,
                    requestType = "PASSWORD_RESET"
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_firebaseConfig.ApiKey}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            LocalStorageHelper.RemoveItem("userId");
            LocalStorageHelper.RemoveItem("idToken");
            LocalStorageHelper.RemoveItem("userRole");
            return true;
        }
    }

    public class FirebaseAuthResponse
    {
        [JsonProperty("idToken")]
        public string IdToken { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("localId")]
        public string LocalId { get; set; }

        [JsonProperty("expiresIn")]
        public string ExpiresIn { get; set; }
    }

    public class FirebaseAuthError
    {
        [JsonProperty("error")]
        public ErrorDetail Error { get; set; }

        public class ErrorDetail
        {
            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}
