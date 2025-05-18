namespace LanguageLearningApp.Models
{
    public class UserBadge
    {
        public string UserId { get; set; }
        public string BadgeId { get; set; }
        public string EarnedDate { get; set; } // hoặc DateTime nếu muốn
    }
}
