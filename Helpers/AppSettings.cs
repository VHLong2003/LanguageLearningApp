using System.Collections.Generic;

namespace LanguageLearningApp.Helpers
{
    public static class AppSettings
    {
        // Firebase config
        public static readonly string FirebaseApiKey = "AIzaSyDcRMJ3I57o6EP1dzGCHorqmubiY6LAZVM";
        public static readonly string FirebaseAuthDomain = "languagelearningapp-f88e7.firebaseapp.com";
        public static readonly string FirebaseProjectId = "languagelearningapp-f88e7";
        public static readonly string FirebaseStorageBucket = "languagelearningapp-f88e7.appspot.com";
        public static readonly string FirebaseDatabaseUrl = "https://languagelearningapp-f88e7-default-rtdb.firebaseio.com/";

        // App settings
        public static readonly int PointsPerCorrectAnswer = 10;
        public static readonly int CoinsPerLessonCompleted = 5;
        public static readonly int DailyGoalPoints = 50;
        public static readonly int DailyLessonsGoal = 5;

        // List of daily motivational messages
        public static readonly List<string> DailyMotivations = new List<string>
        {
            "Consistency is key to mastering a new skill!",
            "Every lesson completed is a step towards fluency.",
            "Learning a little each day adds up to big results.",
            "Your brain is like a muscle - exercise it daily!",
            "The more you practice, the easier it becomes.",
            "Small steps every day lead to big achievements.",
            "Your future self will thank you for learning today.",
            "The best time to learn is always now.",
            "Every expert was once a beginner.",
            "Keep your streak going - you're doing great!"
        };

        // Default badge criteria
        public static readonly Dictionary<string, string> BadgeCriteria = new Dictionary<string, string>
        {
            { "streak", "Maintain a learning streak" },
            { "points", "Earn total points" },
            { "friends", "Connect with other learners" },
            { "course_completion", "Complete an entire course" },
            { "perfect_score", "Get a perfect score on a lesson" },
            { "lessons_completed", "Complete a number of lessons" },
            { "daily_goal", "Complete daily goals" }
        };

        // App version
        public static readonly string AppVersion = "1.0.0";

        // Terms and privacy links
        public static readonly string TermsUrl = "https://example.com/terms";
        public static readonly string PrivacyUrl = "https://example.com/privacy";
    }
}
