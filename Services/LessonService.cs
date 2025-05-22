using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class LessonService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;
        private readonly QuestionService _questionService;

        public LessonService(FirebaseConfig firebaseConfig, QuestionService questionService)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
            _questionService = questionService;
        }

        public async Task<List<LessonModel>> GetLessonsByCourseIdAsync(string courseId, string idToken)
        {
            try
            {
                // Query lessons by courseId
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons.json?orderBy=\"courseId\"&equalTo=\"{courseId}\"&auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var lessonsDictionary = JsonConvert.DeserializeObject<Dictionary<string, LessonModel>>(content);

                    if (lessonsDictionary == null)
                        return new List<LessonModel>();

                    var lessons = new List<LessonModel>();
                    foreach (var kvp in lessonsDictionary)
                    {
                        var lesson = kvp.Value;
                        lesson.LessonId = kvp.Key;

                        // Load questions for this lesson
                        lesson.Questions = await _questionService.GetQuestionsByLessonIdAsync(lesson.LessonId, idToken);

                        lessons.Add(lesson);
                    }

                    // Sort lessons by order
                    lessons.Sort((x, y) => x.Order.CompareTo(y.Order));

                    return lessons;
                }

                return new List<LessonModel>();
            }
            catch
            {
                return new List<LessonModel>();
            }
        }

        public async Task<LessonModel> GetLessonByIdAsync(string lessonId, string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons/{lessonId}.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var lesson = JsonConvert.DeserializeObject<LessonModel>(content);

                    if (lesson != null)
                    {
                        lesson.LessonId = lessonId;

                        // Load questions for this lesson
                        lesson.Questions = await _questionService.GetQuestionsByLessonIdAsync(lessonId, idToken);
                    }

                    return lesson;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> CreateLessonAsync(LessonModel lesson, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(lesson);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons.json?auth={idToken}",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

                    if (result.ContainsKey("name"))
                        return result["name"]; // Firebase generated key
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateLessonAsync(LessonModel lesson, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(lesson);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons/{lesson.LessonId}.json?auth={idToken}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteLessonAsync(string lessonId, string idToken)
        {
            try
            {
                // First delete all questions of this lesson
                var questions = await _questionService.GetQuestionsByLessonIdAsync(lessonId, idToken);
                foreach (var question in questions)
                {
                    await _questionService.DeleteQuestionAsync(question.QuestionId, idToken);
                }

                // Then delete the lesson
                var response = await _httpClient.DeleteAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons/{lessonId}.json?auth={idToken}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<QuestionModel>> GetLessonQuestions(string lessonId, string idToken)
        {
            try
            {
                // Query questions by lessonId
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions.json?orderBy=\"lessonId\"&equalTo=\"{lessonId}\"&auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var questionsDictionary = JsonConvert.DeserializeObject<Dictionary<string, QuestionModel>>(content);

                    if (questionsDictionary == null)
                        return new List<QuestionModel>();

                    var questions = new List<QuestionModel>();
                    foreach (var kvp in questionsDictionary)
                    {
                        var question = kvp.Value;
                        question.QuestionId = kvp.Key;
                        questions.Add(question);
                    }

                    // Sort questions by order
                    questions.Sort((x, y) => x.Order.CompareTo(y.Order));

                    return questions;
                }

                return new List<QuestionModel>();
            }
            catch
            {
                return new List<QuestionModel>();
            }
        }

    }


}
