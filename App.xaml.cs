using LanguageLearningApp.Helpers;
using LanguageLearningApp.Views.Auth;
using LanguageLearningApp.Views.User;
using LanguageLearningApp.Views.Admin;
using System.Globalization;

namespace LanguageLearningApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set app culture (optional - for date/number formatting)
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            // Register routes for navigation
            RegisterRoutes();

            // Create main page (AppShell)
            MainPage = new AppShell();

            // Check if user is already logged in
            CheckLoginStatus();
        }

        private void RegisterRoutes()
        {
            // Auth routes
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("forgotPassword", typeof(ForgotPasswordPage));

            // User routes
            Routing.RegisterRoute("courseDetail", typeof(CourseDetailPage));
            Routing.RegisterRoute("lesson", typeof(LessonPage));
            Routing.RegisterRoute("userProfile", typeof(ProfilePage));

            // Admin routes
            Routing.RegisterRoute("userProgress", typeof(UserProgressPage));
            Routing.RegisterRoute("lessonManagement", typeof(LessonsManagementPage));
            Routing.RegisterRoute("questionManagement", typeof(QuestionsManagementPage));
            Routing.RegisterRoute("badgeManagement", typeof(BadgeManagementPage));
        }

        private void CheckLoginStatus()
        {
            // Check if user is already logged in
            var userId = LocalStorageHelper.GetItem("userId");
            var idToken = LocalStorageHelper.GetItem("idToken");
            var userRole = LocalStorageHelper.GetItem("userRole");

            if (!string.IsNullOrEmpty(userId) &&
                !string.IsNullOrEmpty(idToken) &&
                !string.IsNullOrEmpty(userRole))
            {
                // User already logged in
                if (userRole == "Admin")
                {
                    Shell.Current.GoToAsync("///admin");
                }
                else
                {
                    Shell.Current.GoToAsync("///user");
                }
            }
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);

            // For desktop platforms, set default size
            window.Width = 400;
            window.Height = 800;

            return window;
        }
    }
}
