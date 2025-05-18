using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageLearningApp.Services
{
    public class LessonService
    {
        public async Task<List<Lesson>> GetLessonsByCourseIdAsync(string courseId)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}lessons.json";
            var json = await ApiHelper.GetAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new List<Lesson>();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, Lesson>>(json);
            var list = dict?.Select(kvp => {
                kvp.Value.LessonId = kvp.Key;
                return kvp.Value;
            }).Where(l => l.CourseId == courseId).OrderBy(l => l.Order).ToList() ?? new List<Lesson>();
            return list;
        }

        public async Task AddOrUpdateLessonAsync(Lesson lesson)
        {
            if (string.IsNullOrEmpty(lesson.LessonId))
                lesson.LessonId = Guid.NewGuid().ToString();
            var url = $"{FirebaseConfig.DatabaseUrl}lessons/{lesson.LessonId}.json";
            var json = JsonConvert.SerializeObject(lesson);
            await ApiHelper.PutAsync(url, json);
        }

        public async Task DeleteLessonAsync(string lessonId)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}lessons/{lessonId}.json";
            await ApiHelper.DeleteAsync(url);
        }

        public async Task<Lesson> GetLessonByIdAsync(string lessonId)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}lessons/{lessonId}.json";
            var json = await ApiHelper.GetAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;
            var lesson = JsonConvert.DeserializeObject<Lesson>(json);
            lesson.LessonId = lessonId;
            return lesson;
        }
    }
}
