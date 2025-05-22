using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    public partial class CourseDetailPage : ContentPage
    {
        private readonly CourseDetailViewModel _viewModel;

        // Không nhận courseId ở đây!
        public CourseDetailPage(
            CourseService courseService,
            LessonService lessonService,
            ProgressService progressService,
            UserService userService,
            LessonProgressService lessonProgressService)
        {
            InitializeComponent();
            _viewModel = new CourseDetailViewModel(courseService, lessonService, progressService, userService, lessonProgressService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Khi property CourseId được Shell gán, ViewModel sẽ tự load data
            // Nếu muốn: bạn có thể kiểm tra và load lại (hoặc không cần)
        }
    }
}
