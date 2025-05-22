using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class LeaderboardService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;
        private readonly UserService _userService;
        private readonly ProgressService _progressService;

        public LeaderboardService(FirebaseConfig firebaseConfig, UserService userService, ProgressService progressService)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
            _userService = userService;
            _progressService = progressService;
        }

        public async Task<List<LeaderboardModel>> GetGlobalLeaderboardAsync(string idToken)
        {
            try
            {
                // Get all users
                var users = await _userService.GetAllUsersAsync(idToken);
                var leaderboard = new List<LeaderboardModel>();

                // Calculate total points for each user
                foreach (var user in users)
                {
                    var totalPoints = await _progressService.GetTotalUserPointsAsync(user.UserId, idToken);

                    var entry = new LeaderboardModel
                    {
                        EntryId = Guid.NewGuid().ToString(),
                        UserId = user.UserId,
                        UserName = user.FullName,
                        AvatarUrl = user.AvatarUrl,
                        TotalPoints = totalPoints,
                        LastUpdated = DateTime.Now,
                        Streak = user.CurrentStreak
                    };

                    leaderboard.Add(entry);
                }

                // Sort by points (descending) and assign ranks
                leaderboard = leaderboard.OrderByDescending(e => e.TotalPoints).ToList();

                for (int i = 0; i < leaderboard.Count; i++)
                {
                    leaderboard[i].Rank = i + 1;
                }

                return leaderboard;
            }
            catch
            {
                return new List<LeaderboardModel>();
            }
        }

        public async Task<List<LeaderboardModel>> GetFriendsLeaderboardAsync(string userId, string idToken)
        {
            try
            {
                // Get current user's friends
                var friends = await _userService.GetFriendsAsync(userId, idToken);

                // Add the current user as well
                var currentUser = await _userService.GetUserByIdAsync(userId, idToken);
                if (currentUser != null)
                    friends.Add(currentUser);

                var leaderboard = new List<LeaderboardModel>();

                // Calculate points for each friend
                foreach (var friend in friends)
                {
                    var totalPoints = await _progressService.GetTotalUserPointsAsync(friend.UserId, idToken);

                    var entry = new LeaderboardModel
                    {
                        EntryId = Guid.NewGuid().ToString(),
                        UserId = friend.UserId,
                        UserName = friend.FullName,
                        AvatarUrl = friend.AvatarUrl,
                        TotalPoints = totalPoints,
                        LastUpdated = DateTime.Now,
                        Streak = friend.CurrentStreak
                    };

                    leaderboard.Add(entry);
                }

                // Sort by points (descending) and assign ranks
                leaderboard = leaderboard.OrderByDescending(e => e.TotalPoints).ToList();

                for (int i = 0; i < leaderboard.Count; i++)
                {
                    leaderboard[i].Rank = i + 1;
                }

                return leaderboard;
            }
            catch
            {
                return new List<LeaderboardModel>();
            }
        }

        public async Task<List<LeaderboardModel>> GetWeeklyLeaderboardAsync(string idToken)
        {
            try
            {
                // Get all progress from the last 7 days
                var oneWeekAgo = DateTime.Now.AddDays(-7);

                // Get all users
                var users = await _userService.GetAllUsersAsync(idToken);
                var leaderboard = new List<LeaderboardModel>();

                foreach (var user in users)
                {
                    // Get all progress for this user
                    var allProgress = await _progressService.GetUserProgressAsync(user.UserId, idToken);

                    // Filter for last 7 days
                    var weeklyProgress = allProgress.Where(p => p.CompletedDate >= oneWeekAgo).ToList();

                    // Sum up points
                    int weeklyPoints = weeklyProgress.Sum(p => p.EarnedPoints);

                    var entry = new LeaderboardModel
                    {
                        EntryId = Guid.NewGuid().ToString(),
                        UserId = user.UserId,
                        UserName = user.FullName,
                        AvatarUrl = user.AvatarUrl,
                        TotalPoints = await _progressService.GetTotalUserPointsAsync(user.UserId, idToken),
                        WeeklyPoints = weeklyPoints,
                        LastUpdated = DateTime.Now,
                        Streak = user.CurrentStreak
                    };

                    leaderboard.Add(entry);
                }

                // Sort by weekly points (descending) and assign ranks
                leaderboard = leaderboard.OrderByDescending(e => e.WeeklyPoints).ToList();

                for (int i = 0; i < leaderboard.Count; i++)
                {
                    leaderboard[i].Rank = i + 1;
                }

                return leaderboard;
            }
            catch
            {
                return new List<LeaderboardModel>();
            }
        }

        public async Task<int> GetUserRankAsync(string userId, string idToken)
        {
            try
            {
                var leaderboard = await GetGlobalLeaderboardAsync(idToken);
                var userEntry = leaderboard.FirstOrDefault(e => e.UserId == userId);

                if (userEntry != null)
                    return userEntry.Rank;

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task UpdateLeaderboardSnapshotAsync(string idToken)
        {
            try
            {
                // Get current global leaderboard
                var leaderboard = await GetGlobalLeaderboardAsync(idToken);

                // Save as a snapshot
                var batchUpdate = new Dictionary<string, object>();
                foreach (var entry in leaderboard)
                {
                    batchUpdate[$"/leaderboardSnapshot/{entry.UserId}"] = new
                    {
                        userId = entry.UserId,
                        userName = entry.UserName,
                        avatarUrl = entry.AvatarUrl,
                        totalPoints = entry.TotalPoints,
                        rank = entry.Rank,
                        streak = entry.Streak,
                        lastUpdated = DateTime.Now.ToString("o")
                    };
                }

                var json = JsonConvert.SerializeObject(batchUpdate);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/.json?auth={idToken}",
                    content);
            }
            catch
            {
                // Log error but continue
            }
        }
    }
}
