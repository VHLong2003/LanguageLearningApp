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
                if (string.IsNullOrEmpty(courseId))
                    throw new ArgumentException("CourseId không được để trống hoặc null.", nameof(courseId));

                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống hoặc null.", nameof(idToken));

                // Ghi log yêu cầu
                Console.WriteLine($"Đang lấy danh sách bài học cho CourseId: {courseId}");

                // Truy vấn bài học theo courseId
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons.json?orderBy=\"courseId\"&equalTo=\"{courseId}\"&auth={idToken}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể lấy danh sách bài học cho CourseId {courseId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Dữ liệu thô từ Firebase: {content}");

                var lessonsDictionary = JsonConvert.DeserializeObject<Dictionary<string, LessonModel>>(content);
                if (lessonsDictionary == null || lessonsDictionary.Count == 0)
                {
                    Console.WriteLine($"Không tìm thấy bài học nào cho CourseId: {courseId}");
                    return new List<LessonModel>();
                }

                var lessons = new List<LessonModel>();
                foreach (var kvp in lessonsDictionary)
                {
                    var lesson = kvp.Value;
                    if (lesson == null)
                    {
                        Console.WriteLine($"Bỏ qua bài học null với ID: {kvp.Key}");
                        continue;
                    }

                    lesson.LessonId = kvp.Key;
                    Console.WriteLine($"Đang xử lý bài học: {lesson.Title} (ID: {lesson.LessonId}, CourseId: {lesson.CourseId})");

                    // Tải câu hỏi cho bài học này
                    lesson.Questions = await _questionService.GetQuestionsByLessonIdAsync(lesson.LessonId, idToken);
                    if (lesson.Questions == null)
                    {
                        Console.WriteLine($"Không tìm thấy câu hỏi nào cho LessonId: {lesson.LessonId}");
                        lesson.Questions = new List<QuestionModel>();
                    }

                    lessons.Add(lesson);
                }

                // Sắp xếp bài học theo thứ tự
                lessons.Sort((x, y) => x.Order.CompareTo(y.Order));
                Console.WriteLine($"Trả về {lessons.Count} bài học cho CourseId: {courseId}");

                return lessons;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetLessonsByCourseIdAsync: {ex.Message}");
                throw; // Ném lại ngoại lệ để được xử lý bởi caller
            }
        }

        public async Task<LessonModel> GetLessonByIdAsync(string lessonId, string idToken)
        {
            try
            {
                if (string.IsNullOrEmpty(lessonId))
                    throw new ArgumentException("LessonId không được để Asc hoặc null.", nameof(lessonId));

                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống hoặc null.", nameof(idToken));

                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons/{lessonId}.json?auth={idToken}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể lấy bài học với ID {lessonId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var lesson = JsonConvert.DeserializeObject<LessonModel>(content);

                if (lesson != null)
                {
                    lesson.LessonId = lessonId;
                    lesson.Questions = await _questionService.GetQuestionsByLessonIdAsync(lessonId, idToken);
                    if (lesson.Questions == null)
                    {
                        lesson.Questions = new List<QuestionModel>();
                    }
                }

                return lesson;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetLessonByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<string> CreateLessonAsync(LessonModel lesson, string idToken)
        {
            try
            {
                if (lesson == null)
                    throw new ArgumentNullException(nameof(lesson), "Bài học không được null.");

                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống hoặc null.", nameof(idToken));

                Console.WriteLine($"Đang tạo bài học với CourseId: {lesson.CourseId}, Tiêu đề: {lesson.Title}");

                var json = JsonConvert.SerializeObject(lesson);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons.json?auth={idToken}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể tạo bài học. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

                if (result != null && result.ContainsKey("name"))
                {
                    var lessonId = result["name"];
                    Console.WriteLine($"Bài học đã được tạo với ID: {lessonId}");
                    return lessonId;
                }

                throw new Exception("Không thể phân tích ID bài học từ phản hồi Firebase.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong CreateLessonAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateLessonAsync(LessonModel lesson, string idToken)
        {
            try
            {
                if (lesson == null)
                    throw new ArgumentNullException(nameof(lesson), "Bài học không được null.");

                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống hoặc null.", nameof(idToken));

                var json = JsonConvert.SerializeObject(lesson);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons/{lesson.LessonId}.json?auth={idToken}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể cập nhật bài học với ID {lesson.LessonId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong UpdateLessonAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteLessonAsync(string lessonId, string idToken)
        {
            try
            {
                if (string.IsNullOrEmpty(lessonId))
                    throw new ArgumentException("LessonId không được để trống hoặc null.", nameof(lessonId));

                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống hoặc null.", nameof(idToken));

                // Xóa tất cả câu hỏi của bài học này trước
                var questions = await _questionService.GetQuestionsByLessonIdAsync(lessonId, idToken);
                if (questions != null)
                {
                    foreach (var question in questions)
                    {
                        await _questionService.DeleteQuestionAsync(question.QuestionId, idToken);
                    }
                }

                // Sau đó xóa bài học
                var response = await _httpClient.DeleteAsync(
                    $"{_firebaseConfig.DatabaseUrl}/lessons/{lessonId}.json?auth={idToken}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể xóa bài học với ID {lessonId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong DeleteLessonAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<QuestionModel>> GetLessonQuestions(string lessonId, string idToken)
        {
            try
            {
                if (string.IsNullOrEmpty(lessonId))
                    throw new ArgumentException("LessonId không được để trống hoặc null.", nameof(lessonId));

                if (string.IsNullOrEmpty(idToken))
                    throw new ArgumentException("idToken không được để trống hoặc null.", nameof(idToken));

                // Truy vấn câu hỏi theo lessonId
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/questions.json?orderBy=\"lessonId\"&equalTo=\"{lessonId}\"&auth={idToken}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Không thể lấy danh sách câu hỏi cho LessonId {lessonId}. Trạng thái: {response.StatusCode}, Lý do: {response.ReasonPhrase}, Nội dung: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var questionsDictionary = JsonConvert.DeserializeObject<Dictionary<string, QuestionModel>>(content);

                if (questionsDictionary == null || questionsDictionary.Count == 0)
                {
                    return new List<QuestionModel>();
                }

                var questions = new List<QuestionModel>();
                foreach (var kvp in questionsDictionary)
                {
                    var question = kvp.Value;
                    if (question == null)
                        continue;

                    question.QuestionId = kvp.Key;
                    questions.Add(question);
                }

                // Sắp xếp câu hỏi theo thứ tự
                questions.Sort((x, y) => x.Order.CompareTo(y.Order));
                return questions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong GetLessonQuestions: {ex.Message}");
                throw;
            }
        }
    }
}