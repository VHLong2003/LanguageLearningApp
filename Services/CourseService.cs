using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class CourseService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;

        public CourseService(FirebaseConfig firebaseConfig)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
        }

        // Lấy tất cả khóa học từ cơ sở dữ liệu
        public async Task<List<CourseModel>> GetAllCoursesAsync(string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/courses.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var coursesDictionary = JsonConvert.DeserializeObject<Dictionary<string, CourseModel>>(content);

                    if (coursesDictionary == null)
                        return new List<CourseModel>();

                    var courses = new List<CourseModel>();
                    foreach (var kvp in coursesDictionary)
                    {
                        var course = kvp.Value;
                        course.CourseId = kvp.Key;
                        courses.Add(course);
                    }

                    return courses;
                }

                return new List<CourseModel>();
            }
            catch
            {
                return new List<CourseModel>();
            }
        }

        // Lấy các khóa học đã được công bố
        public async Task<List<CourseModel>> GetPublishedCoursesAsync(string idToken)
        {
            var allCourses = await GetAllCoursesAsync(idToken);
            return allCourses.FindAll(c => c.IsPublished);
        }

        // Lấy khóa học theo ID
        public async Task<CourseModel> GetCourseByIdAsync(string courseId, string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/courses/{courseId}.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var course = JsonConvert.DeserializeObject<CourseModel>(content);

                    if (course != null)
                        course.CourseId = courseId;

                    return course;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Tạo một khóa học mới
        public async Task<string> CreateCourseAsync(CourseModel course, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(course);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_firebaseConfig.DatabaseUrl}/courses.json?auth={idToken}",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

                    if (result.ContainsKey("name"))
                        return result["name"]; // Đây là khóa Firebase được tạo tự động
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Cập nhật thông tin khóa học
        public async Task<bool> UpdateCourseAsync(CourseModel course, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(course);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/courses/{course.CourseId}.json?auth={idToken}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Xóa một khóa học
        public async Task<bool> DeleteCourseAsync(string courseId, string idToken)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"{_firebaseConfig.DatabaseUrl}/courses/{courseId}.json?auth={idToken}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Lấy các khóa học theo loại
        public async Task<List<CourseModel>> GetCoursesByTypeAsync(CourseType type, string idToken)
        {
            var allCourses = await GetPublishedCoursesAsync(idToken);
            return allCourses.FindAll(c => c.Type == type);
        }
    }
}