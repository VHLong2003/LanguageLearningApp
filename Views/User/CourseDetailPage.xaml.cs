using LanguageLearningApp.Models;
using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    public partial class CourseDetailPage : ContentPage
    {
        public CourseDetailPage(Course course)
        {
            InitializeComponent();
            var vm = BindingContext as CourseDetailViewModel;
            if (vm != null)
                vm.Course = course;
        }
    }
}


