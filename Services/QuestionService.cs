using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class QuestionService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;

        public QuestionService(FirebaseConfig firebaseConfig)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
        }

        public async Task<List<QuestionModel>> GetQuestionsByLessonIdAsync(string lessonId, string idToken)
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

        public async Task<QuestionModel> GetQuestionByIdAsync(string questionId, string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions/{questionId}.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var question = JsonConvert.DeserializeObject<QuestionModel>(content);

                    if (question != null)
                        question.QuestionId = questionId;

                    return question;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> CreateQuestionAsync(QuestionModel question, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(question);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions.json?auth={idToken}",
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

        public async Task<bool> UpdateQuestionAsync(QuestionModel question, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(question);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions/{question.QuestionId}.json?auth={idToken}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteQuestionAsync(string questionId, string idToken)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions/{questionId}.json?auth={idToken}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReorderQuestionsAsync(List<QuestionModel> questions, string idToken)
        {
            try
            {
                // Update the order of each question
                for (int i = 0; i < questions.Count; i++)
                {
                    questions[i].Order = i + 1;
                    await UpdateQuestionAsync(questions[i], idToken);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
