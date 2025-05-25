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
    public class LessonProgressService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;

        public LessonProgressService(FirebaseConfig firebaseConfig)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
        }

        public async Task<LessonProgressModel> GetLessonProgressAsync(string userId, string lessonId, string idToken)
        {
            try
            {
                // Query specific lesson progress
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessonProgress.json?orderBy=\"userId\"&equalTo=\"{userId}\"&auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var progressDictionary = JsonConvert.DeserializeObject<Dictionary<string, LessonProgressModel>>(content);

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

        public async Task<List<LessonProgressModel>> GetUserLessonProgressAsync(string userId, string courseId, string idToken)
        {
            try
            {
                // Query all progress for a user in a specific course
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessonProgress.json?orderBy=\"userId\"&equalTo=\"{userId}\"&auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var progressDictionary = JsonConvert.DeserializeObject<Dictionary<string, LessonProgressModel>>(content);

                    if (progressDictionary == null)
                        return new List<LessonProgressModel>();

                    var progressList = new List<LessonProgressModel>();
                    foreach (var kvp in progressDictionary)
                    {
                        var progress = kvp.Value;
                        if (progress.CourseId == courseId)
                        {
                            progress.ProgressId = kvp.Key;
                            progressList.Add(progress);
                        }
                    }

                    return progressList;
                }

                return new List<LessonProgressModel>();
            }
            catch
            {
                return new List<LessonProgressModel>();
            }
        }

        public async Task<string> CreateOrUpdateLessonProgressAsync(LessonProgressModel progress, string idToken)
        {
            try
            {
                // Check if progress entry already exists
                var existingProgress = await GetLessonProgressAsync(progress.UserId, progress.LessonId, idToken);

                if (existingProgress != null)
                {
                    // Update existing progress
                    progress.ProgressId = existingProgress.ProgressId;

                    var json = JsonConvert.SerializeObject(progress);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PatchAsync(
                        $"{_firebaseConfig.DatabaseUrl}/lessonProgress/{progress.ProgressId}.json?auth={idToken}",
                        content);

                    return existingProgress.ProgressId;
                }
                else
                {
                    // Create new progress entry
                    var json = JsonConvert.SerializeObject(progress);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(
                        $"{_firebaseConfig.DatabaseUrl}/lessonProgress.json?auth={idToken}",
                        content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

                        if (result.ContainsKey("name"))
                        {
                            return result["name"]; // Firebase generated ID
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

        public async Task<bool> CompleteLessonProgressAsync(string userId, string lessonId, string courseId, int correctAnswers, int totalQuestions, int earnedPoints, int timeSpent, string idToken)
        {
            try
            {
                var progress = await GetLessonProgressAsync(userId, lessonId, idToken);

                if (progress == null)
                {
                    // Create new completed progress
                    progress = new LessonProgressModel
                    {
                        UserId = userId,
                        LessonId = lessonId,
                        CourseId = courseId,
                        StartedDate = DateTime.Now.AddMinutes(-1 * timeSpent / 60), // Estimate start time
                        LastAccessDate = DateTime.Now,
                        CompletedDate = DateTime.Now,
                        IsCompleted = true,
                        PercentComplete = 100,
                        CurrentQuestionIndex = totalQuestions,
                        TotalQuestions = totalQuestions,
                        CorrectAnswers = correctAnswers,
                        EarnedPoints = earnedPoints,
                        TimeSpent = timeSpent,
                        Attempts = 1
                    };
                }
                else
                {
                    // Update existing progress as completed
                    progress.LastAccessDate = DateTime.Now;
                    progress.CompletedDate = DateTime.Now;
                    progress.IsCompleted = true;
                    progress.PercentComplete = 100;
                    progress.CurrentQuestionIndex = totalQuestions;
                    progress.CorrectAnswers = correctAnswers;
                    progress.EarnedPoints = earnedPoints;
                    progress.TimeSpent = timeSpent;
                    progress.Attempts += 1;
                }

                var progressId = await CreateOrUpdateLessonProgressAsync(progress, idToken);
                return progressId != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProgressStatusAsync(string userId, string lessonId, int currentQuestionIndex, double percentComplete, string idToken)
        {
            try
            {
                var progress = await GetLessonProgressAsync(userId, lessonId, idToken);

                if (progress != null)
                {
                    // Update progress status
                    progress.LastAccessDate = DateTime.Now;
                    progress.CurrentQuestionIndex = currentQuestionIndex;
                    progress.PercentComplete = percentComplete;

                    var progressId = await CreateOrUpdateLessonProgressAsync(progress, idToken);
                    return progressId != null;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<double> GetOverallCourseProgressAsync(string userId, string courseId, string idToken)
        {
            try
            {
                var courseProgress = await GetUserLessonProgressAsync(userId, courseId, idToken);

                if (courseProgress.Count == 0)
                    return 0;

                // Calculate percentage of completed lessons
                var completedLessons = courseProgress.Count(p => p.IsCompleted);
                var totalLessons = await GetTotalLessonsInCourseAsync(courseId, idToken);

                if (totalLessons == 0)
                    return 0;

                return (double)completedLessons / totalLessons * 100;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetTotalLessonsInCourseAsync(string courseId, string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/courses/{courseId}/totalLessons.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var totalLessons = JsonConvert.DeserializeObject<int>(content);
                    return totalLessons;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }
        public async Task<bool> UpdateLessonProgressAsync(LessonProgressModel progress, string idToken)
        {
            try
            {
                if (progress == null)
                    throw new ArgumentNullException(nameof(progress), "Tiến trình bài học không được null.");

                if (string.IsNullOrEmpty(progress.UserId))
                    throw new ArgumentException("UserId không được để trống hoặc null.", nameof(progress.UserId));

                if (string.IsNullOrEmpty(progress.LessonId))
                    throw new ArgumentException("LessonId không được để trống hoặc null.", nameof(progress.LessonId));

                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống hoặc null.", nameof(idToken));

                Console.WriteLine($"Đang cập nhật tiến trình bài học cho UserId: {progress.UserId}, LessonId: {progress.LessonId}");

                var json = JsonConvert.SerializeObject(progress);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessonProgress/{progress.UserId}/{progress.LessonId}.json?auth={idToken}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể cập nhật tiến trình bài học. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                Console.WriteLine($"Tiến trình bài học đã được cập nhật: {json}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong UpdateLessonProgressAsync: {ex.Message}");
                throw;
            }
        }
    }
}
