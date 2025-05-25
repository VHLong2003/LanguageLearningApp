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
        private readonly CourseService _courseService; // Dịch vụ khóa học
        private readonly UserService _userService; // Dịch vụ người dùng
        private readonly ProgressService _progressService; // Dịch vụ tiến độ

        private ObservableCollection<CourseModel> _courses; // Danh sách khóa học
        private ObservableCollection<CourseType> _categories; // Danh sách danh mục
        private CourseType _selectedCategory; // Danh mục được chọn
        private bool _isRefreshing; // Trạng thái làm mới
        private string _searchQuery; // Từ khóa tìm kiếm
        private UsersModel _currentUser; // Người dùng hiện tại
        private string searchQuery; // Biến tìm kiếm (trùng lặp, có thể là lỗi)

        public ObservableCollection<CourseModel> Courses
        {
            get => _courses;
            set => SetProperty(ref _courses, value); // Cập nhật danh sách khóa học
        }

        public ObservableCollection<CourseType> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value); // Cập nhật danh sách danh mục
        }

        public CourseType SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    FilterCourses(); // Lọc khóa học khi danh mục thay đổi
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value); // Cập nhật trạng thái làm mới
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                    FilterCourses(); // Lọc khóa học khi từ khóa tìm kiếm thay đổi
            }
        }

        public ICommand RefreshCommand { get; } // Lệnh làm mới
        public ICommand CourseSelectedCommand { get; } // Lệnh chọn khóa học
        public ICommand LoadCoursesCommand { get; } // Lệnh tải khóa học
        public ICommand SearchCoursesCommand { get; } // Lệnh tìm kiếm khóa học
        public ICommand FilterCoursesByCategoryCommand { get; } // Lệnh lọc theo danh mục
        private ObservableCollection<CourseModel> _featuredCourses = new ObservableCollection<CourseModel>(); // Danh sách khóa học nổi bật
        public ObservableCollection<CourseModel> FeaturedCourses
        {
            get => _featuredCourses;
            set => SetProperty(ref _featuredCourses, value); // Cập nhật danh sách khóa học nổi bật
        }

        public CoursesViewModel(CourseService courseService, UserService userService, ProgressService progressService)
        {
            _courseService = courseService;
            _userService = userService;
            _progressService = progressService;

            Courses = new ObservableCollection<CourseModel>(); // Khởi tạo danh sách khóa học

            // Khởi tạo danh mục
            Categories = new ObservableCollection<CourseType>
            {
                CourseType.Language, // Language
                CourseType.Programming, // Programming
                CourseType.Science, // Science
                CourseType.Mathematics, // Mathematics
                CourseType.Art, // Art
                CourseType.Music, // Music
                CourseType.History, // History
                CourseType.Other // Other
            };

            RefreshCommand = new Command(async () => await LoadCoursesAsync()); // Gán lệnh làm mới
            CourseSelectedCommand = new Command<CourseModel>(async (course) => await OnCourseSelected(course)); // Gán lệnh chọn khóa học
            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync()); // Gán lệnh tải khóa học
            SearchCoursesCommand = new Command(() => FilterCourses()); // Gán lệnh tìm kiếm
            FilterCoursesByCategoryCommand = new Command<CourseType>((category) =>
            {
                SelectedCategory = category; // Cập nhật danh mục được chọn
                // FilterCourses(); // Lọc khóa học (bị comment)
            });
        }

        public async Task InitializeAsync()
        {
            // Tải dữ liệu người dùng
            var userId = LocalStorageHelper.GetItem("userId"); // Lấy ID người dùng
            var idToken = LocalStorageHelper.GetItem("idToken"); // Lấy token xác thực

            _currentUser = await _userService.GetUserByIdAsync(userId, idToken); // Lấy thông tin người dùng

            // Tải danh sách khóa học
            await LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            IsRefreshing = true; // Bật trạng thái làm mới

            var idToken = LocalStorageHelper.GetItem("idToken"); // Lấy token xác thực
            var allCourses = await _courseService.GetPublishedCoursesAsync(idToken); // Lấy danh sách khóa học đã xuất bản

            Courses.Clear(); // Xóa danh sách khóa học hiện tại
            foreach (var course in allCourses)
            {
                Courses.Add(course); // Thêm từng khóa học vào danh sách
            }

            IsRefreshing = false; // Tắt trạng thái làm mới
            var featured = allCourses.FindAll(c => c.IsFeatured); // Lấy các khóa học nổi bật
            FeaturedCourses = new ObservableCollection<CourseModel>(featured); // Cập nhật danh sách khóa học nổi bật
        }

        private void FilterCourses()
        {
            IsRefreshing = true; // Bật trạng thái làm mới

            var idToken = LocalStorageHelper.GetItem("idToken"); // Lấy token xác thực
            Task.Run(async () =>
            {
                var allCourses = await _courseService.GetPublishedCoursesAsync(idToken); // Lấy danh sách khóa học

                // Lọc theo danh mục nếu có
                if (SelectedCategory != default)
                {
                    allCourses = allCourses.FindAll(c => c.Type == SelectedCategory); // Lọc theo danh mục được chọn
                }

                // Lọc theo từ khóa tìm kiếm nếu có
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var query = SearchQuery.ToLower(); // Chuyển từ khóa về chữ thường
                    allCourses = allCourses.FindAll(c =>
                        c.Title.ToLower().Contains(query) ||
                        c.Description.ToLower().Contains(query)); // Lọc theo tiêu đề hoặc mô tả
                }

                // Cập nhật giao diện trên luồng chính
                Device.BeginInvokeOnMainThread(() =>
                {
                    Courses.Clear(); // Xóa danh sách khóa học hiện tại
                    foreach (var course in allCourses)
                    {
                        Courses.Add(course); // Thêm từng khóa học vào danh sách
                    }

                    IsRefreshing = false; // Tắt trạng thái làm mới
                });
            });
        }

        public async Task OnCourseSelected(CourseModel course)
        {
            if (course == null) return; // Thoát nếu khóa học rỗng

            // Kiểm tra xem người dùng có đủ điểm để mở khóa học không
            if (_currentUser.Points < course.RequiredPointsToUnlock)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Khóa học bị khóa",
                    $"Bạn cần {course.RequiredPointsToUnlock} điểm để mở khóa học này. Bạn hiện có {_currentUser.Points} điểm.",
                    "OK"); // Hiển thị thông báo nếu không đủ điểm
                return;
            }

            // Chuyển hướng đến trang chi tiết khóa học
            await Shell.Current.GoToAsync($"courseDetail?courseId={course.CourseId}");
        }
    }
}