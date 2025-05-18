namespace LanguageLearningApp.Models
{
    public class Badge
    {
        public string BadgeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public BadgeType Type { get; set; }
        public int TargetValue { get; set; } // Ví dụ: 10 bài học, 7 ngày liên tục...
    }

    public enum BadgeType
    {
        CompleteLessonCount,      // Hoàn thành X bài học
        CorrectAnswersCount,      // Đúng X câu hỏi
        StreakDay,                // Đăng nhập liên tục X ngày
        // Thêm các loại khác nếu muốn
    }
}
