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

            // Register routes
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("forgot_password", typeof(ForgotPasswordPage));
            Routing.RegisterRoute("course_detail", typeof(CourseDetailPage));
            Routing.RegisterRoute("lesson", typeof(LessonPage));
            Routing.RegisterRoute("shop", typeof(ShopPage));

            // Admin routes
            Routing.RegisterRoute("user_details", typeof(UsersManagementPage));
            Routing.RegisterRoute("course_editor", typeof(CoursesManagementPage));
            Routing.RegisterRoute("lesson_editor", typeof(LessonsManagementPage));
            Routing.RegisterRoute("question_editor", typeof(QuestionsManagementPage));
        }
    }
}