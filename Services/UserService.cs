using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;

        public UserService(FirebaseConfig firebaseConfig)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
        }

        public async Task<UsersModel> GetUserByIdAsync(string userId, string idToken)
        {
            try
            {
                // Set authorization header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {idToken}");

                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/users/{userId}.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<UsersModel>(content);
                    return user;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(UsersModel user, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(
                    $"{_firebaseConfig.DatabaseUrl}/users/{user.UserId}.json?auth={idToken}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(UsersModel user, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/users/{user.UserId}.json?auth={idToken}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddFriendAsync(string userId, string friendId, string idToken)
        {
            try
            {
                // Get current user
                var user = await GetUserByIdAsync(userId, idToken);
                if (user == null) return false;

                // Add friend to list
                if (user.FriendIds == null)
                    user.FriendIds = new List<string>();

                if (!user.FriendIds.Contains(friendId))
                    user.FriendIds.Add(friendId);

                // Update user
                return await UpdateUserAsync(user, idToken);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFriendAsync(string userId, string friendId, string idToken)
        {
            try
            {
                // Get current user
                var user = await GetUserByIdAsync(userId, idToken);
                if (user == null || user.FriendIds == null) return false;

                // Remove friend from list
                user.FriendIds.Remove(friendId);

                // Update user
                return await UpdateUserAsync(user, idToken);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UsersModel>> GetFriendsAsync(string userId, string idToken)
        {
            var friends = new List<UsersModel>();

            try
            {
                // Get current user
                var user = await GetUserByIdAsync(userId, idToken);
                if (user == null || user.FriendIds == null || user.FriendIds.Count == 0)
                    return friends;

                // Get each friend's details
                foreach (var friendId in user.FriendIds)
                {
                    var friend = await GetUserByIdAsync(friendId, idToken);
                    if (friend != null)
                        friends.Add(friend);
                }

                return friends;
            }
            catch
            {
                return friends;
            }
        }

        public async Task<bool> AddPointsAsync(string userId, int points, string idToken)
        {
            try
            {
                // Get current user
                var user = await GetUserByIdAsync(userId, idToken);
                if (user == null) return false;

                // Add points
                user.Points += points;
                user.LastActive = DateTime.Now;

                // Update user
                return await UpdateUserAsync(user, idToken);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddCoinsAsync(string userId, int coins, string idToken)
        {
            try
            {
                // Get current user
                var user = await GetUserByIdAsync(userId, idToken);
                if (user == null) return false;

                // Add coins
                user.Coins += coins;

                // Update user
                return await UpdateUserAsync(user, idToken);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UsersModel>> GetAllUsersAsync(string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/users.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var usersDictionary = JsonConvert.DeserializeObject<Dictionary<string, UsersModel>>(content);

                    var users = new List<UsersModel>();
                    foreach (var kvp in usersDictionary)
                    {
                        var user = kvp.Value;
                        user.UserId = kvp.Key;
                        users.Add(user);
                    }

                    return users;
                }

                return new List<UsersModel>();
            }
            catch
            {
                return new List<UsersModel>();
            }
        }
    }
}
