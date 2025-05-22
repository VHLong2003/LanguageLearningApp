using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class BadgeService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;
        private readonly UserService _userService;

        public BadgeService(FirebaseConfig firebaseConfig, UserService userService)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
            _userService = userService;
        }

        public async Task<List<BadgeModel>> GetAllBadgesAsync(string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/badges.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var badgesDictionary = JsonConvert.DeserializeObject<Dictionary<string, BadgeModel>>(content);

                    if (badgesDictionary == null)
                        return new List<BadgeModel>();

                    var badges = new List<BadgeModel>();
                    foreach (var kvp in badgesDictionary)
                    {
                        var badge = kvp.Value;
                        badge.BadgeId = kvp.Key;
                        badges.Add(badge);
                    }

                    return badges;
                }

                return new List<BadgeModel>();
            }
            catch
            {
                return new List<BadgeModel>();
            }
        }

        public async Task<List<BadgeModel>> GetUserBadgesAsync(string userId, string idToken)
        {
            try
            {
                // Get user first to get badge IDs
                var user = await _userService.GetUserByIdAsync(userId, idToken);
                if (user == null || user.BadgeIds == null || user.BadgeIds.Count == 0)
                    return new List<BadgeModel>();

                // Get all badges
                var allBadges = await GetAllBadgesAsync(idToken);

                // Filter badges that the user has
                var userBadges = new List<BadgeModel>();
                foreach (var badge in allBadges)
                {
                    if (user.BadgeIds.Contains(badge.BadgeId))
                        userBadges.Add(badge);
                }

                return userBadges;
            }
            catch
            {
                return new List<BadgeModel>();
            }
        }

        public async Task<BadgeModel> GetBadgeByIdAsync(string badgeId, string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/badges/{badgeId}.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var badge = JsonConvert.DeserializeObject<BadgeModel>(content);

                    if (badge != null)
                        badge.BadgeId = badgeId;

                    return badge;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> CreateBadgeAsync(BadgeModel badge, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(badge);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_firebaseConfig.DatabaseUrl}/badges.json?auth={idToken}",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

                    if (result.ContainsKey("name"))
                        return result["name"];
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateBadgeAsync(BadgeModel badge, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(badge);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/badges/{badge.BadgeId}.json?auth={idToken}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBadgeAsync(string badgeId, string idToken)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"{_firebaseConfig.DatabaseUrl}/badges/{badgeId}.json?auth={idToken}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AwardBadgeToUserAsync(string badgeId, string userId, string idToken)
        {
            try
            {
                // Get user
                var user = await _userService.GetUserByIdAsync(userId, idToken);
                if (user == null)
                    return false;

                // Check if user already has this badge
                if (user.BadgeIds == null)
                    user.BadgeIds = new List<string>();

                if (user.BadgeIds.Contains(badgeId))
                    return true; // Already has the badge

                // Get the badge to see rewards
                var badge = await GetBadgeByIdAsync(badgeId, idToken);
                if (badge == null)
                    return false;

                // Add badge to user
                user.BadgeIds.Add(badgeId);

                // Add badge rewards
                user.Points += badge.PointsReward;
                user.Coins += badge.CoinsReward;

                // Update user
                return await _userService.UpdateUserAsync(user, idToken);
            }
            catch
            {
                return false;
            }
        }

        public async Task CheckAndAwardBadges(UsersModel user, string idToken)
        {
            try
            {
                // Get all badges
                var badges = await GetAllBadgesAsync(idToken);

                foreach (var badge in badges)
                {
                    // Skip if user already has this badge
                    if (user.BadgeIds != null && user.BadgeIds.Contains(badge.BadgeId))
                        continue;

                    // Check badge criteria
                    bool shouldAward = false;

                    switch (badge.Criteria)
                    {
                        case "streak":
                            // Award if user's streak meets or exceeds required value
                            shouldAward = user.CurrentStreak >= badge.RequiredValue;
                            break;

                        case "points":
                            // Award if user's points meet or exceed required value
                            shouldAward = user.Points >= badge.RequiredValue;
                            break;

                        case "friends":
                            // Award if user has enough friends
                            shouldAward = user.FriendIds != null && user.FriendIds.Count >= badge.RequiredValue;
                            break;

                            // Add more criteria types as needed
                    }

                    if (shouldAward)
                    {
                        await AwardBadgeToUserAsync(badge.BadgeId, user.UserId, idToken);
                    }
                }
            }
            catch
            {
                // Log error but continue
            }
        }

        public async Task AwardCourseCompletionBadge(string userId, string courseId, string idToken)
        {
            try
            {
                // Find a badge with criteria "course_completion" and the course ID
                var badges = await GetAllBadgesAsync(idToken);
                var courseBadge = badges.Find(b => b.Criteria == "course_completion" && b.RequiredValue.ToString() == courseId);

                if (courseBadge != null)
                {
                    await AwardBadgeToUserAsync(courseBadge.BadgeId, userId, idToken);
                }
            }
            catch
            {
                // Log error but continue
            }
        }
    }
}
