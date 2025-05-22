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
    public class ProgressService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;
        private readonly BadgeService _badgeService;
        private readonly UserService _userService;

        public ProgressService(FirebaseConfig firebaseConfig, BadgeService badgeService, UserService userService)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
            _badgeService = badgeService;
            _userService = userService;
        }

        public async Task<List<ProgressModel>> GetUserProgressAsync(string userId, string idToken)
        {
            try
            {
                // Query progress by userId
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/progress.json?orderBy=\"userId\"&equalTo=\"{userId}\"&auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var progressDictionary = JsonConvert.DeserializeObject<Dictionary<string, ProgressModel>>(content);

                    if (progressDictionary == null)
                        return new List<ProgressModel>();

                    var progressList = new List<ProgressModel>();
                    foreach (var kvp in progressDictionary)
                    {
                        var progress = kvp.Value;
                        progress.ProgressId = kvp.Key;
                        progressList.Add(progress);
                    }

                    return progressList;
                }

                return new List<ProgressModel>();
            }
            catch
            {
                return new List<ProgressModel>();
            }
        }

        public async Task<ProgressModel> GetLessonProgressAsync(string userId, string lessonId, string idToken)
        {
            try
            {
                // Query specific lesson progress
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/progress.json?orderBy=\"userId\"&equalTo=\"{userId}\"&auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var progressDictionary = JsonConvert.DeserializeObject<Dictionary<string, ProgressModel>>(content);

                    if (progressDictionary == null)
                        return null;

                    foreach (var kvp in progressDictionary)
                    {
                        var progress = kvp.Value;
                        if (progress.LessonId == lessonId)
                        {
                            progress.ProgressId = kvp.Key;
                            return progress;
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<double> GetCourseProgressPercentAsync(string userId, string courseId, string idToken)
        {
            try
            {
                // Get all user progress for this course
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/progress.json?orderBy=\"userId\"&equalTo=\"{userId}\"&auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var progressDictionary = JsonConvert.DeserializeObject<Dictionary<string, ProgressModel>>(content);

                    if (progressDictionary == null)
                        return 0;

                    // Count completed lessons for this course
                    int completedLessons = 0;
                    foreach (var kvp in progressDictionary)
                    {
                        var progress = kvp.Value;
                        if (progress.CourseId == courseId && progress.PercentComplete == 100)
                            completedLessons++;
                    }

                    // Get total lessons for this course
                    var courseResponse = await _httpClient.GetAsync(
                        $"{_firebaseConfig.DatabaseUrl}/courses/{courseId}.json?auth={idToken}");

                    if (courseResponse.IsSuccessStatusCode)
                    {
                        var courseContent = await courseResponse.Content.ReadAsStringAsync();
                        var course = JsonConvert.DeserializeObject<CourseModel>(courseContent);

                        if (course != null && course.TotalLessons > 0)
                            return (double)completedLessons / course.TotalLessons * 100;
                    }
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<string> SaveProgressAsync(ProgressModel progress, string idToken)
        {
            try
            {
                // Check if progress already exists
                var existingProgress = await GetLessonProgressAsync(progress.UserId, progress.LessonId, idToken);

                if (existingProgress != null)
                {
                    // Update existing progress
                    progress.ProgressId = existingProgress.ProgressId;
                    var json = JsonConvert.SerializeObject(progress);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PutAsync(
                        $"{_firebaseConfig.DatabaseUrl}/progress/{progress.ProgressId}.json?auth={idToken}",
                        content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Check for badges and update user points
                        await ProcessProgressAchievements(progress, idToken);
                        return progress.ProgressId;
                    }
                }
                else
                {
                    // Create new progress
                    var json = JsonConvert.SerializeObject(progress);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(
                        $"{_firebaseConfig.DatabaseUrl}/progress.json?auth={idToken}",
                        content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

                        if (result.ContainsKey("name"))
                        {
                            // Check for badges and update user points
                            progress.ProgressId = result["name"];
                            await ProcessProgressAchievements(progress, idToken);
                            return result["name"];
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task ProcessProgressAchievements(ProgressModel progress, string idToken)
        {
            try
            {
                // Update user points
                await _userService.AddPointsAsync(progress.UserId, progress.EarnedPoints, idToken);

                // Check for streak
                var user = await _userService.GetUserByIdAsync(progress.UserId, idToken);
                if (user != null)
                {
                    // Check if last activity was yesterday
                    if ((DateTime.Now - user.LastActive).TotalHours <= 48 &&
                        (DateTime.Now - user.LastActive).TotalHours >= 12)
                    {
                        user.CurrentStreak++;
                    }
                    else if ((DateTime.Now - user.LastActive).TotalHours > 48)
                    {
                        // Reset streak if more than 2 days since last activity
                        user.CurrentStreak = 1;
                    }

                    // Update last active
                    user.LastActive = DateTime.Now;
                    await _userService.UpdateUserAsync(user, idToken);

                    // Check for streak badges
                    await _badgeService.CheckAndAwardBadges(user, idToken);
                }

                // Check for course completion
                double courseProgress = await GetCourseProgressPercentAsync(progress.UserId, progress.CourseId, idToken);
                if (courseProgress >= 100)
                {
                    // Award course completion badge if it exists
                    await _badgeService.AwardCourseCompletionBadge(progress.UserId, progress.CourseId, idToken);
                }
            }
            catch
            {
                // Log error but continue
            }
        }

        public async Task<bool> DeleteProgressAsync(string progressId, string idToken)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"{_firebaseConfig.DatabaseUrl}/progress/{progressId}.json?auth={idToken}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetTotalUserPointsAsync(string userId, string idToken)
        {
            try
            {
                var allProgress = await GetUserProgressAsync(userId, idToken);
                int totalPoints = 0;

                foreach (var progress in allProgress)
                {
                    totalPoints += progress.EarnedPoints;
                }

                return totalPoints;
            }
            catch
            {
                return 0;
            }
        }
    }
}
