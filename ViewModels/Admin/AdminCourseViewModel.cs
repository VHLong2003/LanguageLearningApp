using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.Admin
{
    public class AdminCourseViewModel : BaseViewModel
    {
        private readonly CourseService _courseService;
        private readonly StorageService _storageService;

        private ObservableCollection<CourseModel> _courses;
        private CourseModel _selectedCourse;
        private bool _isNewCourse;
        private bool _isEditing;
        private bool _isRefreshing;
        private bool _isBusy;
        private string _errorMessage;

        // Các trường biểu mẫu
        private string _title;
        private string _description;
        private CourseType _type;
        private int _difficultyLevel;
        private int _requiredPoints;
        private string _imageUrl;
        private bool _isPublished;

        public ObservableCollection<CourseModel> Courses
        {
            get => _courses;
            set => SetProperty(ref _courses, value);
        }

        public CourseModel SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (SetProperty(ref _selectedCourse, value) && value != null)
                {
                    Title = value.Title;
                    Description = value.Description;
                    Type = value.Type;
                    DifficultyLevel = value.DifficultyLevel;
                    RequiredPoints = value.RequiredPointsToUnlock;
                    ImageUrl = value.ImageUrl;
                    IsPublished = value.IsPublished;

                    IsEditing = true;
                    IsNewCourse = false;
                }
            }
        }

        public bool IsNewCourse
        {
            get => _isNewCourse;
            set => SetProperty(ref _isNewCourse, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public CourseType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public int DifficultyLevel
        {
            get => _difficultyLevel;
            set => SetProperty(ref _difficultyLevel, value);
        }

        public int RequiredPoints
        {
            get => _requiredPoints;
            set => SetProperty(ref _requiredPoints, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public bool IsPublished
        {
            get => _isPublished;
            set => SetProperty(ref _isPublished, value);
        }

        public Array CourseTypes => Enum.GetValues(typeof(CourseType));

        public ICommand LoadCoursesCommand { get; }
        public ICommand NewCourseCommand { get; }
        public ICommand SaveCourseCommand { get; }
        public ICommand DeleteCourseCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand ManageLessonsCommand { get; }

        public AdminCourseViewModel(CourseService courseService, StorageService storageService)
        {
            _courseService = courseService;
            _storageService = storageService;

            Courses = new ObservableCollection<CourseModel>();

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            NewCourseCommand = new Command(CreateNewCourse);
            SaveCourseCommand = new Command(async () => await SaveCourseAsync());
            DeleteCourseCommand = new Command<CourseModel>(async (course) => await DeleteCourseAsync(course));
            CancelEditCommand = new Command(CancelEdit);
            PickImageCommand = new Command(async () => await PickImageAsync());
            ManageLessonsCommand = new Command<CourseModel>(async (course) => await NavigateToLessonsAsync(course));
        }

        public async Task InitializeAsync()
        {
            await LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            IsRefreshing = true;
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực.");

                var allCourses = await _courseService.GetAllCoursesAsync(idToken);
                Courses.Clear();
                foreach (var course in allCourses)
                {
                    Courses.Add(course);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải khóa học: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void CreateNewCourse()
        {
            Title = string.Empty;
            Description = string.Empty;
            Type = CourseType.Language;
            DifficultyLevel = 1;
            RequiredPoints = 0;
            ImageUrl = string.Empty;
            IsPublished = false;

            IsNewCourse = true;
            IsEditing = true;
            SelectedCourse = null;
            ErrorMessage = string.Empty;
        }

        private async Task SaveCourseAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Tiêu đề là bắt buộc.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Mô tả là bắt buộc.";
                return;
            }

            if (DifficultyLevel < 1 || DifficultyLevel > 10)
            {
                ErrorMessage = "Mức độ khó phải nằm trong khoảng từ 1 đến 10.";
                return;
            }

            if (RequiredPoints < 0)
            {
                ErrorMessage = "Điểm yêu cầu không được âm.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                if (IsNewCourse)
                {
                    var newCourse = new CourseModel
                    {
                        Title = Title,
                        Description = Description,
                        Type = Type,
                        DifficultyLevel = DifficultyLevel,
                        RequiredPointsToUnlock = RequiredPoints,
                        ImageUrl = ImageUrl,
                        IsPublished = IsPublished,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now.ToString("o"),
                        TotalLessons = 0
                    };

                    var courseId = await _courseService.CreateCourseAsync(newCourse, idToken);
                    if (courseId != null)
                    {
                        newCourse.CourseId = courseId;
                        Courses.Add(newCourse);
                        CancelEdit();
                    }
                    else
                    {
                        ErrorMessage = "Không thể tạo khóa học.";
                    }
                }
                else
                {
                    if (SelectedCourse != null)
                    {
                        SelectedCourse.Title = Title;
                        SelectedCourse.Description = Description;
                        SelectedCourse.Type = Type;
                        SelectedCourse.DifficultyLevel = DifficultyLevel;
                        SelectedCourse.RequiredPointsToUnlock = RequiredPoints;
                        SelectedCourse.ImageUrl = ImageUrl;
                        SelectedCourse.IsPublished = IsPublished;

                        var success = await _courseService.UpdateCourseAsync(SelectedCourse, idToken);
                        if (success)
                        {
                            CancelEdit();
                        }
                        else
                        {
                            ErrorMessage = "Không thể cập nhật khóa học.";
                        }
                    }
                }

                await LoadCoursesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi lưu khóa học: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteCourseAsync(CourseModel course)
        {
            if (course == null) return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Xác nhận xóa",
                $"Bạn có chắc chắn muốn xóa '{course.Title}'? Thao tác này cũng sẽ xóa tất cả bài học và câu hỏi liên quan.",
                "Có", "Không");

            if (!confirm) return;

            IsBusy = true;
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (!string.IsNullOrEmpty(course.ImageUrl))
                {
                    await _storageService.DeleteImageAsync(course.ImageUrl, idToken);
                }

                var success = await _courseService.DeleteCourseAsync(course.CourseId, idToken);
                if (success)
                {
                    Courses.Remove(course);
                    if (SelectedCourse == course)
                    {
                        CancelEdit();
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể xóa khóa học.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi xóa khóa học: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            IsNewCourse = false;
            SelectedCourse = null;
            ErrorMessage = string.Empty;

            Title = string.Empty;
            Description = string.Empty;
            Type = CourseType.Language;
            DifficultyLevel = 1;
            RequiredPoints = 0;
            ImageUrl = string.Empty;
            IsPublished = false;
        }

        private async Task PickImageAsync()
        {
            try
            {
                if (!await CheckNetworkConnectionAsync())
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không có kết nối internet. Vui lòng kiểm tra mạng và thử lại.", "OK");
                    return;
                }

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Chọn hình ảnh khóa học"
                });

                if (result != null)
                {
                    IsBusy = true;
                    var idToken = LocalStorageHelper.GetItem("idToken");
                    if (string.IsNullOrEmpty(idToken))
                    {
                        await App.Current.MainPage.DisplayAlert("Lỗi", "Người dùng chưa được xác thực. Vui lòng đăng nhập lại.", "OK");
                        return;
                    }

                    var imageUrl = await _storageService.UploadImageFromPickerAsync(result, "course_images", idToken);
                    if (imageUrl != null)
                    {
                        ImageUrl = imageUrl;
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể tải lên hình ảnh. Vui lòng thử lại.", "OK");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                var message = ex.Message.Contains("403") ? "Truy cập bị từ chối: Vui lòng kiểm tra quy tắc Firebase Storage hoặc mã xác thực của bạn."
                    : ex.Message.Contains("401") ? "Xác thực thất bại: Vui lòng đăng nhập lại."
                    : $"Lỗi khi tải lên hình ảnh: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Lỗi", message, "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi chọn hình ảnh: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NavigateToLessonsAsync(CourseModel course)
        {
            if (course == null)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không có khóa học được chọn.", "OK");
                return;
            }

            try
            {
                await Shell.Current.GoToAsync($"lessonManagement?courseId={course.CourseId}");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể điều hướng đến quản lý bài học: {ex.Message}", "OK");
            }
        }

        private async Task<bool> CheckNetworkConnectionAsync()
        {
            var current = Connectivity.NetworkAccess;
            return current == NetworkAccess.Internet;
        }
    }
}