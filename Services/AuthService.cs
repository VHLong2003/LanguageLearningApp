using System.Threading.Tasks;
using LanguageLearningApp.Models;
using LanguageLearningApp.Helpers;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class AuthService
    {
        private const string SignUpUrl =
            "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" + FirebaseConfig.ApiKey;
        private const string SignInUrl =
            "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" + FirebaseConfig.ApiKey;

        /// Đăng ký tài khoản mới với Email/Password (Firebase Auth)
        public async Task<FirebaseAuthResponse> RegisterAsync(string email, string password)
        {
            var data = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var json = JsonConvert.SerializeObject(data);
            var response = await ApiHelper.PostAsync(SignUpUrl, json);

            if (response.Contains("error"))
                throw new Exception(GetFirebaseErrorMessage(response));

            return JsonConvert.DeserializeObject<FirebaseAuthResponse>(response);
        }

        /// Đăng nhập bằng Email/Password (Firebase Auth)
        public async Task<FirebaseAuthResponse> LoginAsync(string email, string password)
        {
            var data = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var json = JsonConvert.SerializeObject(data);
            var response = await ApiHelper.PostAsync(SignInUrl, json);

            if (response.Contains("error"))
                throw new Exception(GetFirebaseErrorMessage(response));

            return JsonConvert.DeserializeObject<FirebaseAuthResponse>(response);
        }

        private string GetFirebaseErrorMessage(string jsonResponse)
        {
            // Đọc lỗi từ response
            dynamic errorObj = JsonConvert.DeserializeObject(jsonResponse);
            return errorObj?.error?.message ?? "Firebase Auth Error";
        }
    }
}
