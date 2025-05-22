using System.Collections.Generic;
using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public class LessonModel
    {
        [JsonProperty("lessonId")]
        public string LessonId { get; set; }

        [JsonProperty("courseId")]
        public string CourseId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("videoUrl")]
        public string VideoUrl { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("requiredPointsToUnlock")]
        public int RequiredPointsToUnlock { get; set; }

        [JsonProperty("maxPoints")]
        public int MaxPoints { get; set; }

        [JsonProperty("estimatedMinutes")]
        public int EstimatedMinutes { get; set; }

        // Navigation property - not stored in Firebase
        [JsonIgnore]
        public List<QuestionModel> Questions { get; set; }
    }
}
