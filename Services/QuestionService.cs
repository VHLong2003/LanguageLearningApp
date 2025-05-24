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
                if (string.IsNullOrEmpty(lessonId))
                    throw new ArgumentException("LessonId không được để trống.", nameof(lessonId));
                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống.", nameof(idToken));

                Console.WriteLine($"Lấy câu hỏi cho LessonId: {lessonId}");
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions.json?orderBy=\"lessonId\"&equalTo=\"{lessonId}\"&auth={idToken}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể lấy câu hỏi cho LessonId {lessonId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Phản hồi từ Firebase: {content}");

                var questionsDictionary = JsonConvert.DeserializeObject<Dictionary<string, QuestionModel>>(content);
                if (questionsDictionary == null || questionsDictionary.Count == 0)
                {
                    Console.WriteLine($"Không tìm thấy câu hỏi cho LessonId: {lessonId}");
                    return new List<QuestionModel>();
                }

                var questions = new List<QuestionModel>();
                foreach (var kvp in questionsDictionary)
                {
                    var question = kvp.Value;
                    if (question == null) continue;
                    question.QuestionId = kvp.Key;
                    Console.WriteLine($"Xử lý câu hỏi: {question.Content} (ID: {question.QuestionId})");
                    questions.Add(question);
                }

                questions.Sort((x, y) => x.Order.CompareTo(y.Order));
                Console.WriteLine($"Trả về {questions.Count} câu hỏi cho LessonId: {lessonId}");
                return questions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetQuestionsByLessonIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<QuestionModel> GetQuestionByIdAsync(string questionId, string idToken)
        {
            try
            {
                if (string.IsNullOrEmpty(questionId))
                    throw new ArgumentException("QuestionId không được để trống.", nameof(questionId));
                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống.", nameof(idToken));

                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions/{questionId}.json?auth={idToken}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể lấy câu hỏi với ID {questionId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var question = JsonConvert.DeserializeObject<QuestionModel>(content);
                if (question != null) question.QuestionId = questionId;
                return question;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetQuestionByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<string> CreateQuestionAsync(QuestionModel question, string idToken)
        {
            try
            {
                if (question == null)
                    throw new ArgumentNullException(nameof(question));
                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống.", nameof(idToken));

                Console.WriteLine($"Tạo câu hỏi mới cho LessonId: {question.LessonId}, Nội dung: {question.Content}");
                var json = JsonConvert.SerializeObject(question);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions.json?auth={idToken}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể tạo câu hỏi. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                if (result?.ContainsKey("name") == true)
                {
                    var questionId = result["name"];
                    Console.WriteLine($"Câu hỏi được tạo với ID: {questionId}");
                    return questionId;
                }
                throw new Exception("Không thể phân tích ID câu hỏi từ phản hồi Firebase.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong CreateQuestionAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateQuestionAsync(QuestionModel question, string idToken)
        {
            try
            {
                if (question == null)
                    throw new ArgumentNullException(nameof(question));
                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống.", nameof(idToken));

                var json = JsonConvert.SerializeObject(question);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions/{question.QuestionId}.json?auth={idToken}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể cập nhật câu hỏi với ID {question.QuestionId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong UpdateQuestionAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteQuestionAsync(string questionId, string idToken)
        {
            try
            {
                if (string.IsNullOrEmpty(questionId))
                    throw new ArgumentException("QuestionId không được để trống.", nameof(questionId));
                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống.", nameof(idToken));

                var response = await _httpClient.DeleteAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions/{questionId}.json?auth={idToken}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể xóa câu hỏi với ID {questionId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong DeleteQuestionAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ReorderQuestionsAsync(List<QuestionModel> questions, string idToken)
        {
            try
            {
                for (int i = 0; i < questions.Count; i++)
                {
                    questions[i].Order = i + 1;
                    await UpdateQuestionAsync(questions[i], idToken);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong ReorderQuestionsAsync: {ex.Message}");
                throw;
            }
        }
    }
}