using System.Collections.Generic;
using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        FillInTheBlank,
        Matching,
        ShortAnswer,
        VoiceRecording,
        Arrangement
    }

    public class QuestionModel
    {
        [JsonProperty("questionId")]
        public string QuestionId { get; set; }

        [JsonProperty("lessonId")]
        public string LessonId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("audioUrl")]
        public string AudioUrl { get; set; }

        [JsonProperty("type")]
        public QuestionType Type { get; set; }

        [JsonProperty("options")]
        public List<string> Options { get; set; }

        [JsonProperty("correctAnswer")]
        public string CorrectAnswer { get; set; }

        [JsonProperty("explanation")]
        public string Explanation { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("timeLimit")]
        public int TimeLimit { get; set; } // In seconds
    }
}
