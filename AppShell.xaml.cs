using LanguageLearningApp.Views.User;
using LanguageLearningApp.Views.Admin;
using LanguageLearningApp.Views.Auth;

namespace LanguageLearningApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for authentication
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("forgotPassword", typeof(ForgotPasswordPage)); 

            // Register routes for user pages
            Routing.RegisterRoute("courseDetail", typeof(CourseDetailPage)); 
            Routing.RegisterRoute("lesson", typeof(LessonPage));
            Routing.RegisterRoute("userProfile", typeof(ProfilePage)); 
            Routing.RegisterRoute("shop", typeof(ShopPage));

            // Register routes for admin pages
            Routing.RegisterRoute("userProgress", typeof(UserProgressPage)); 
            Routing.RegisterRoute("lessonManagement", typeof(LessonsManagementPage)); 
            Routing.RegisterRoute("questionManagement", typeof(QuestionsManagementPage)); 
            Routing.RegisterRoute("badgeManagement", typeof(BadgeManagementPage)); 
        }
    }
}