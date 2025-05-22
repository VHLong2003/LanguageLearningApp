using System;
using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public class LessonProgressModel
    {
        [JsonProperty("progressId")]
        public string ProgressId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("lessonId")]
        public string LessonId { get; set; }

        [JsonProperty("courseId")]
        public string CourseId { get; set; }

        [JsonProperty("startedDate")]
        public DateTime StartedDate { get; set; }

        [JsonProperty("lastAccessDate")]
        public DateTime LastAccessDate { get; set; }

        [JsonProperty("completedDate")]
        public DateTime? CompletedDate { get; set; }

        [JsonProperty("isCompleted")]
        public bool IsCompleted { get; set; }

        [JsonProperty("percentComplete")]
        public double PercentComplete { get; set; }

        [JsonProperty("currentQuestionIndex")]
        public int CurrentQuestionIndex { get; set; }

        [JsonProperty("totalQuestions")]
        public int TotalQuestions { get; set; }

        [JsonProperty("correctAnswers")]
        public int CorrectAnswers { get; set; }

        [JsonProperty("earnedPoints")]
        public int EarnedPoints { get; set; }

        [JsonProperty("timeSpent")]
        public int TimeSpent { get; set; } // in seconds

        [JsonProperty("attempts")]
        public int Attempts { get; set; }
    }
}
