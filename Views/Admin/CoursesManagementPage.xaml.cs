using LanguageLearningApp.Models;
using LanguageLearningApp.ViewModels.Admin;
using LanguageLearningApp.Views.Admin;

namespace LanguageLearningApp.Views.Admin
{
    public partial class CoursesManagementPage : ContentPage
    {
        private AdminCourseViewModel ViewModel => BindingContext as AdminCourseViewModel;
        public CoursesManagementPage()
        {
            InitializeComponent();
            if (ViewModel != null)
            {
                ViewModel.CourseSelected += OnCourseSelected;
            }
        }

        private async void OnCourseSelected(object sender, Course course)
        {
            if (course != null)
                await Navigation.PushAsync(new LessonsManagementPage(course.CourseId));
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Để tránh highlight khi chọn, không cần code gì cũng được hoặc có thể call logic ở đây nếu muốn
        }
    }
}
