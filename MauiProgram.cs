using LanguageLearningApp.Helpers;
using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.User;
using LanguageLearningApp.ViewModels.Admin;
using Microsoft.Extensions.Logging;

namespace LanguageLearningApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Firebase configuration
            builder.Services.AddSingleton<FirebaseConfig>();

            // Register services
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<EmailService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<CourseService>();
            builder.Services.AddSingleton<LessonService>();
            builder.Services.AddSingleton<QuestionService>();
            builder.Services.AddSingleton<ProgressService>();
            builder.Services.AddSingleton<LeaderboardService>();
            builder.Services.AddSingleton<BadgeService>();
            builder.Services.AddSingleton<ShopService>();
            builder.Services.AddSingleton<StorageService>();
            builder.Services.AddSingleton<LessonProgressService>();

            // Register ViewModels
            builder.Services.AddTransient<AuthViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<CoursesViewModel>();
            builder.Services.AddTransient<CourseDetailViewModel>();
            builder.Services.AddTransient<LessonViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<LeaderboardViewModel>();
            builder.Services.AddTransient<ShopViewModel>();
            builder.Services.AddTransient<LessonProgressViewModel>();


            // Admin ViewModels
            builder.Services.AddTransient<AdminHomeViewModel>();
            builder.Services.AddTransient<AdminUserViewModel>();
            builder.Services.AddTransient<AdminCourseViewModel>();
            builder.Services.AddTransient<AdminLessonViewModel>();
            builder.Services.AddTransient<AdminQuestionViewModel>();
            builder.Services.AddTransient<AdminShopViewModel>();
            builder.Services.AddTransient<AdminBadgeViewModel>();

            // Register views
            // Auth pages
            builder.Services.AddTransient<Views.Auth.LoginPage>();
            builder.Services.AddTransient<Views.Auth.RegisterPage>();
            builder.Services.AddTransient<Views.Auth.ForgotPasswordPage>();

            // User pages
            builder.Services.AddTransient<Views.User.HomeUserPage>();
            builder.Services.AddTransient<Views.User.CoursesPage>();
            builder.Services.AddTransient<Views.User.CourseDetailPage>();
            builder.Services.AddTransient<Views.User.LessonPage>();
            builder.Services.AddTransient<Views.User.ProfilePage>();
            builder.Services.AddTransient<Views.User.LeaderboardPage>();
            builder.Services.AddTransient<Views.User.ShopPage>();
            builder.Services.AddTransient<Views.User.LessonProgressPage>();


            // Admin pages
            builder.Services.AddTransient<Views.Admin.HomeAdminPage>();
            builder.Services.AddTransient<Views.Admin.UsersManagementPage>();
            builder.Services.AddTransient<Views.Admin.CoursesManagementPage>();
            builder.Services.AddTransient<Views.Admin.LessonsManagementPage>();
            builder.Services.AddTransient<Views.Admin.QuestionsManagementPage>();
            builder.Services.AddTransient<Views.Admin.ShopManagementPage>();
            builder.Services.AddTransient<Views.Admin.BadgeManagementPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
