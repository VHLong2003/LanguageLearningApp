namespace LanguageLearningApp.Models
{
    public class Question
    {
        public string QuestionId { get; set; }
        public string LessonId { get; set; }
        public string Content { get; set; }
        public QuestionType Type { get; set; }
        public List<string> Options { get; set; } = new(); // Trắc nghiệm
        public string CorrectAnswer { get; set; }
        public int Points { get; set; }
    }

    public enum QuestionType
    {
        MultipleChoice,
        FillInTheBlank,
        DragAndDrop,
        TrueFalse
    }
    
}
