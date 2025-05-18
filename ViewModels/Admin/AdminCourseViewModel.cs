using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace LanguageLearningApp.ViewModels.Admin
{
    public partial class AdminCourseViewModel : ObservableObject
    {
        private readonly CourseService _courseService = new CourseService();

        [ObservableProperty] private ObservableCollection<Course> courses = new();
        [ObservableProperty] private Course selectedCourse = new();
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private string searchText;

        public event EventHandler<Course> CourseSelected;

        public AdminCourseViewModel()
        {
            _ = LoadCoursesAsync();
        }

        [RelayCommand]
        public async Task LoadCoursesAsync()
        {
            IsBusy = true;
            ErrorMessage = "";
            try
            {
                var all = await _courseService.GetAllCoursesAsync();
                Courses = new ObservableCollection<Course>(all);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task AddOrUpdateCourseAsync()
        {
            if (SelectedCourse == null || string.IsNullOrWhiteSpace(SelectedCourse.Title))
            {
                ErrorMessage = "Nhập tiêu đề khóa học!";
                return;
            }
            IsBusy = true;
            try
            {
                await _courseService.AddOrUpdateCourseAsync(SelectedCourse);
                await LoadCoursesAsync();
                SelectedCourse = new Course();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteCourseAsync(Course course)
        {
            if (course == null) return;
            IsBusy = true;
            try
            {
                await _courseService.DeleteCourseAsync(course.CourseId);
                Courses.Remove(course);
                if (SelectedCourse == course) SelectedCourse = new Course();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void EditCourse(Course course)
        {
            if (course != null)
            {
                SelectedCourse = new Course
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    ImageUrl = course.ImageUrl,
                    Type = course.Type
                };
            }
        }

        [RelayCommand]
        public void SearchCourses()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                _ = LoadCoursesAsync();
            }
            else
            {
                var filtered = Courses.Where(c => (c.Title?.ToLower().Contains(SearchText.ToLower()) ?? false)).ToList();
                Courses = new ObservableCollection<Course>(filtered);
            }
        }

        [RelayCommand]
        public void SelectCourse(Course course)
        {
            CourseSelected?.Invoke(this, course);
        }
    }
}
