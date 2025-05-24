using System.Collections.Generic;
using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    // Enum định nghĩa các loại câu hỏi
    public enum QuestionType
    {
        MultipleChoice,    // Trắc nghiệm
        TrueFalse,         // Đúng/Sai
        FillInTheBlank,    // Điền vào chỗ trống
        Matching,          // Ghép đôi
        ShortAnswer,       // Trả lời ngắn
        VoiceRecording,    // Ghi âm
        Arrangement        // Sắp xếp
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
        public int TimeLimit { get; set; } // Đơn vị: giây
    }
}