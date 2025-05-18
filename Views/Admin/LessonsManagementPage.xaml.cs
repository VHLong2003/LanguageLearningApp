using LanguageLearningApp.Models;
using LanguageLearningApp.ViewModels.Admin;

namespace LanguageLearningApp.Views.Admin
{
    public partial class LessonsManagementPage : ContentPage
    {
        private AdminLessonViewModel ViewModel => BindingContext as AdminLessonViewModel;
        public LessonsManagementPage(string courseId)
        {
            InitializeComponent();
            BindingContext = new AdminLessonViewModel(courseId);
            if (ViewModel != null)
                ViewModel.LessonSelected += OnLessonSelected;
        }

        private async void OnLessonSelected(object sender, Lesson lesson)
        {
            if (lesson != null)
                await Navigation.PushAsync(new QuestionsManagementPage(lesson.LessonId));
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Nếu bạn muốn click chọn là chuyển trang, bỏ hoặc tuỳ chỉnh tại đây
        }
    }
}
