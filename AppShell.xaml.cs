using LanguageLearningApp.ViewModels.User;
using LanguageLearningApp.Views.User;

namespace LanguageLearningApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(CourseDetailPage), typeof(CourseDetailPage));
        }
    }
}
