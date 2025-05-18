
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Views.User
{
    public partial class HomePage : ContentPage
    {
        public List<string> CourseTypes { get; set; }

        public HomePage()
        {
            InitializeComponent();
            CourseTypes = Enum.GetNames(typeof(CourseType)).ToList();
            BindingContext = this;
        }

        private async void OnCourseTypeSelected(object sender, SelectionChangedEventArgs e)
        {
            var type = e.CurrentSelection.FirstOrDefault()?.ToString();
            if (!string.IsNullOrEmpty(type))
            {
                Application.Current.MainPage = new Views.User.CourseListPage(type);
            }
        }
    }
}
