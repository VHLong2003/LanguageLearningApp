using System;
using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public class ProgressModel
    {
        [JsonProperty("progressId")]
        public string ProgressId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("courseId")]
        public string CourseId { get; set; }

        [JsonProperty("lessonId")]
        public string LessonId { get; set; }

        [JsonProperty("completedDate")]
        public DateTime CompletedDate { get; set; }

        [JsonProperty("earnedPoints")]
        public int EarnedPoints { get; set; }

        [JsonProperty("percentComplete")]
        public double PercentComplete { get; set; }

        [JsonProperty("correctAnswers")]
        public int CorrectAnswers { get; set; }

        [JsonProperty("totalQuestions")]
        public int TotalQuestions { get; set; }

        [JsonProperty("timeSpent")]
        public int TimeSpent { get; set; } // In seconds

        [JsonProperty("lastAttemptDate")]
        public DateTime LastAttemptDate { get; set; }
    }
}
