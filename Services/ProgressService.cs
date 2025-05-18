using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace LanguageLearningApp.Services
{
    public class ProgressService
    {
        public async Task SaveProgressAsync(string userId, UserProgress progress)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}progress/{userId}/{progress.LessonId}.json";
            var json = JsonConvert.SerializeObject(progress);
            await ApiHelper.PutAsync(url, json);
        }

        public async Task<List<UserProgress>> GetProgressAsync(string userId)
        {
            var url = $"{FirebaseConfig.DatabaseUrl}progress/{userId}.json";
            var json = await ApiHelper.GetAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new List<UserProgress>();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, UserProgress>>(json);
            return dict?.Values.ToList() ?? new List<UserProgress>();
        }
    }
}
