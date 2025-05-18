namespace LanguageLearningApp.Models
{
    public class AppUser
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string Role { get; set; }
        public int Points { get; set; }
        public List<string> FriendIds { get; set; } = new();
        public List<string> BadgeIds { get; set; } = new();
        public List<UserProgress> Progress { get; set; } = new();
    }
}
