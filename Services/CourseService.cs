using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageLearningApp.Services
{
    public class CourseService
    {
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            var url = $"{FirebaseConfig.DatabaseUrl}courses.json";
            var json = await ApiHelper.GetAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new List<Course>();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, Course>>(json);
            var list = dict?.Select(kvp => { kvp.Value.CourseId = kvp.Key; return kvp.Value; }).ToList() ?? new List<Course>();
            return list;
        }

        public async Task AddOrUpdateCourseAsync(Course course)
        {
            if (string.IsNullOrEmpty(course.CourseId))
                course.CourseId = Guid.NewGuid().ToString();
            var url = $"{FirebaseConfig.DatabaseUrl}courses/{course.CourseId}.json";
            var json = JsonConvert.SerializeObject(course);
            await ApiHelper.PutAsync(url, json);
        }

        public async Task DeleteCourseAsync(string courseId)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}courses/{courseId}.json";
            await ApiHelper.DeleteAsync(url);
        }
    }
}
