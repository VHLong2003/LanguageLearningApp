using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace LanguageLearningApp.Services
{
    public class UserService
    {
        // Lấy user info từ DB (dùng IdToken cho xác thực, userId là LocalId/FirebaseUid)
        public async Task<AppUser> GetUserByIdAsync(string userId, string idToken)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}users/{userId}.json?auth={idToken}";
            var json = await ApiHelper.GetAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;
            return JsonConvert.DeserializeObject<AppUser>(json);
        }

        // Gọi khi đăng ký user thành công để lưu vào Database
        public async Task SaveUserAsync(AppUser user, string idToken)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}users/{user.UserId}.json?auth={idToken}";
            var json = JsonConvert.SerializeObject(user);
            await ApiHelper.PutAsync(url, json);
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            var url = $"{FirebaseConfig.DatabaseUrl}users.json";
            var json = await ApiHelper.GetAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new List<AppUser>();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, AppUser>>(json);
            return dict?.Values.ToList() ?? new List<AppUser>();
        }

        public async Task DeleteUserAsync(string userId)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}users/{userId}.json";
            await ApiHelper.DeleteAsync(url);
        }

    }
}
