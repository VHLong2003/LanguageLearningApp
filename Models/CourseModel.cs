using System.Collections.Generic;
using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public enum CourseType
    {
        Language,
        Programming,
        Science,
        Mathematics,
        Art,
        Music,
        History,
        Other
    }

    public class CourseModel
    {
        [JsonProperty("courseId")]
        public string CourseId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("type")]
        public CourseType Type { get; set; }

        [JsonProperty("difficultyLevel")]
        public int DifficultyLevel { get; set; } 

        [JsonProperty("requiredPoints")]
        public int RequiredPointsToUnlock { get; set; }

        [JsonProperty("totalLessons")]
        public int TotalLessons { get; set; }

        [JsonProperty("isPublished")]
        public bool IsPublished { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; } // Admin ID

        [JsonProperty("createdDate")]
        public string CreatedDate { get; set; }

        // Navigation property - not stored in Firebase
        [JsonIgnore]
        public List<LessonModel> Lessons { get; set; }
        public bool IsFeatured { get; set; }
        public string Level { get; set; }

    }
}
