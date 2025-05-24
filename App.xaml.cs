using LanguageLearningApp.Helpers;
using LanguageLearningApp.Views.Auth;
using LanguageLearningApp.Views.User;
using LanguageLearningApp.Views.Admin;
using System.Globalization;

namespace LanguageLearningApp
{
    public partial class App : Application
    {
        // Property cho Dependency Injection, dùng ở mọi page qua App.Services
        public static IServiceProvider Services { get; private set; }

        // Constructor nhận DI từ MauiProgram
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            Services = serviceProvider;

            // Set app culture (nếu muốn)
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            // Tạo MainPage
            MainPage = new AppShell();

            // Đăng ký routes (nếu dùng Shell)
            RegisterRoutes();

            // Check login tự động nếu đã lưu trạng thái
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
            Routing.RegisterRoute("shop", typeof(ShopPage)); // Nếu có
            // Admin routes
            Routing.RegisterRoute("userProgress", typeof(UserProgressPage));
            Routing.RegisterRoute("lessonManagement", typeof(LessonsManagementPage));
            Routing.RegisterRoute("questionManagement", typeof(QuestionsManagementPage));
            Routing.RegisterRoute("badgeManagement", typeof(BadgeManagementPage));
        }

        private void CheckLoginStatus()
        {
            var userId = LocalStorageHelper.GetItem("userId");
            var idToken = LocalStorageHelper.GetItem("idToken");
            var userRole = LocalStorageHelper.GetItem("userRole");

            if (!string.IsNullOrEmpty(userId) &&
                !string.IsNullOrEmpty(idToken) &&
                !string.IsNullOrEmpty(userRole))
            {
                if (userRole == "Admin")
                    Shell.Current.GoToAsync("///admin");
                else
                    Shell.Current.GoToAsync("///user");
            }
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);
            window.Width = 400;
            window.Height = 800;
            return window;
        }
    }
}
