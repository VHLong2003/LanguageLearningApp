using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.User
{
    public class CoursesViewModel : BaseViewModel
    {
        private readonly CourseService _courseService;
        private readonly UserService _userService;
        private readonly ProgressService _progressService;

        private ObservableCollection<CourseModel> _courses;
        private ObservableCollection<CourseType> _categories;
        private CourseType _selectedCategory;
        private bool _isRefreshing;
        private string _searchQuery;
        private UsersModel _currentUser;
        private string searchQuery;

        public ObservableCollection<CourseModel> Courses
        {
            get => _courses;
            set => SetProperty(ref _courses, value);
        }

        public ObservableCollection<CourseType> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public CourseType SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    FilterCourses();
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                    FilterCourses();
            }
        }



        public ICommand RefreshCommand { get; }
        public ICommand CourseSelectedCommand { get; }
        public ICommand LoadCoursesCommand { get; }
        public ICommand SearchCoursesCommand { get; }
        public ICommand FilterCoursesByCategoryCommand { get; }
        private ObservableCollection<CourseModel> _featuredCourses = new ObservableCollection<CourseModel>();
        public ObservableCollection<CourseModel> FeaturedCourses
        {
            get => _featuredCourses;
            set => SetProperty(ref _featuredCourses, value);
        }


        public CoursesViewModel(CourseService courseService, UserService userService, ProgressService progressService)
        {
            _courseService = courseService;
            _userService = userService;
            _progressService = progressService;

            Courses = new ObservableCollection<CourseModel>();


            // Initialize categories
            Categories = new ObservableCollection<CourseType>
            {
                CourseType.Language,
                CourseType.Programming,
                CourseType.Science,
                CourseType.Mathematics,
                CourseType.Art,
                CourseType.Music,
                CourseType.History,
                CourseType.Other
            };

            RefreshCommand = new Command(async () => await LoadCoursesAsync());
            CourseSelectedCommand = new Command<CourseModel>(async (course) => await OnCourseSelected(course));
            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            SearchCoursesCommand = new Command(() => FilterCourses());
            FilterCoursesByCategoryCommand = new Command<CourseType>((category) =>
            {
                SelectedCategory = category;
                // FilterCourses();
            });


        }

        public async Task InitializeAsync()
        {
            // Load user data
            var userId = LocalStorageHelper.GetItem("userId");
            var idToken = LocalStorageHelper.GetItem("idToken");

            _currentUser = await _userService.GetUserByIdAsync(userId, idToken);

            // Load courses
            await LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            IsRefreshing = true;

            var idToken = LocalStorageHelper.GetItem("idToken");
            var allCourses = await _courseService.GetPublishedCoursesAsync(idToken);

            Courses.Clear();
            foreach (var course in allCourses)
            {
                Courses.Add(course);
            }

            IsRefreshing = false;
            var featured = allCourses.FindAll(c => c.IsFeatured); 
            FeaturedCourses = new ObservableCollection<CourseModel>(featured);
        }

        private void FilterCourses()
        {
            IsRefreshing = true;

            var idToken = LocalStorageHelper.GetItem("idToken");
            Task.Run(async () =>
            {
                var allCourses = await _courseService.GetPublishedCoursesAsync(idToken);

                // Filter by selected category if any
                if (SelectedCategory != default)
                {
                    allCourses = allCourses.FindAll(c => c.Type == SelectedCategory);
                }

                // Filter by search query if any
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var query = SearchQuery.ToLower();
                    allCourses = allCourses.FindAll(c =>
                        c.Title.ToLower().Contains(query) ||
                        c.Description.ToLower().Contains(query));
                }

                // Update on UI thread
                Device.BeginInvokeOnMainThread(() =>
                {
                    Courses.Clear();
                    foreach (var course in allCourses)
                    {
                        Courses.Add(course);
                    }

                    IsRefreshing = false;
                });
            });
        }

        public async Task OnCourseSelected(CourseModel course)
        {
            if (course == null) return;

            // Check if user has enough points to unlock this course
            if (_currentUser.Points < course.RequiredPointsToUnlock)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Course Locked",
                    $"You need {course.RequiredPointsToUnlock} points to unlock this course. You currently have {_currentUser.Points} points.",
                    "OK");
                return;
            }

            // Navigate to course detail page
            await Shell.Current.GoToAsync($"courseDetail?courseId={course.CourseId}");
        }
    }
}
