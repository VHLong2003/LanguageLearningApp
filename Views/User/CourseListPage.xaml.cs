using LanguageLearningApp.Models;
using LanguageLearningApp.Services;

namespace LanguageLearningApp.Views.User
{
    public partial class CourseListPage : ContentPage
    {
        public List<Course> Courses { get; set; }
        public string Title { get; set; }

        public CourseListPage(string type)
        {
            InitializeComponent();
            Title = $"Khóa học: {type}";
            LoadCourses(type);
            BindingContext = this;
        }

        private async void LoadCourses(string type)
        {
            var allCourses = await new CourseService().GetAllCoursesAsync();
            Courses = allCourses.Where(c => c.Type.ToString() == type).ToList();
            OnPropertyChanged(nameof(Courses));
        }

        private async void OnCourseSelected(object sender, SelectionChangedEventArgs e)
        {
            var course = e.CurrentSelection.FirstOrDefault() as Course;
            if (course != null)
            {
                Application.Current.MainPage = new Views.User.CourseDetailPage(course);
            }
        }
    }
}
