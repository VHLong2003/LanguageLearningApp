using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageLearningApp.Services
{
    public class QuestionService
    {
        public async Task<List<Question>> GetQuestionsByLessonIdAsync(string lessonId)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}questions.json";
            var json = await ApiHelper.GetAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new List<Question>();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, Question>>(json);
            var list = dict?.Select(kvp => {
                kvp.Value.QuestionId = kvp.Key;
                return kvp.Value;
            }).Where(q => q.LessonId == lessonId).ToList() ?? new List<Question>();
            return list;
        }

        public async Task AddOrUpdateQuestionAsync(Question question)
        {
            if (string.IsNullOrEmpty(question.QuestionId))
                question.QuestionId = Guid.NewGuid().ToString();
            var url = $"{FirebaseConfig.DatabaseUrl}questions/{question.QuestionId}.json";
            var json = JsonConvert.SerializeObject(question);
            await ApiHelper.PutAsync(url, json);
        }

        public async Task DeleteQuestionAsync(string questionId)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}questions/{questionId}.json";
            await ApiHelper.DeleteAsync(url);
        }

    }
}
