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
    [QueryProperty(nameof(CourseId), "courseId")]
    public class AdminLessonViewModel : BaseViewModel
    {
        private readonly LessonService _lessonService;
        private readonly CourseService _courseService;
        private readonly StorageService _storageService;

        private string _courseId;
        private CourseModel _currentCourse;
        private ObservableCollection<LessonModel> _lessons;
        private LessonModel _selectedLesson;
        private bool _isRefreshing;
        private bool _isLoading;
        private bool _isEditing;
        private bool _isNewLesson;
        private string _errorMessage;

        // Các trường biểu mẫu để chỉnh sửa/tạo bài học
        private string _lessonId;
        private string _title;
        private string _description;
        private string _content;
        private string _imageUrl;
        private string _videoUrl;
        private int _order;
        private int _requiredPoints;
        private int _maxPoints;
        private int _estimatedMinutes;

        public string CourseId
        {
            get => _courseId;
            set
            {
                if (SetProperty(ref _courseId, value) && !string.IsNullOrEmpty(value))
                {
                    Task.Run(async () => await LoadCourseAndLessonsAsync());
                }
            }
        }

        public CourseModel CurrentCourse
        {
            get => _currentCourse;
            set => SetProperty(ref _currentCourse, value);
        }

        public ObservableCollection<LessonModel> Lessons
        {
            get => _lessons;
            set => SetProperty(ref _lessons, value);
        }

        public LessonModel SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                if (SetProperty(ref _selectedLesson, value) && value != null)
                {
                    LessonId = value.LessonId;
                    Title = value.Title;
                    Description = value.Description;
                    Content = value.Content;
                    ImageUrl = value.ImageUrl;
                    VideoUrl = value.VideoUrl;
                    Order = value.Order;
                    RequiredPoints = value.RequiredPointsToUnlock;
                    MaxPoints = value.MaxPoints;
                    EstimatedMinutes = value.EstimatedMinutes;

                    IsEditing = true;
                    IsNewLesson = false;
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsNewLesson
        {
            get => _isNewLesson;
            set => SetProperty(ref _isNewLesson, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string LessonId
        {
            get => _lessonId;
            set => SetProperty(ref _lessonId, value);
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

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public string VideoUrl
        {
            get => _videoUrl;
            set => SetProperty(ref _videoUrl, value);
        }

        public int Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public int RequiredPoints
        {
            get => _requiredPoints;
            set => SetProperty(ref _requiredPoints, value);
        }

        public int MaxPoints
        {
            get => _maxPoints;
            set => SetProperty(ref _maxPoints, value);
        }

        public int EstimatedMinutes
        {
            get => _estimatedMinutes;
            set => SetProperty(ref _estimatedMinutes, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand CreateLessonCommand { get; }
        public ICommand SaveLessonCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteLessonCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand ManageQuestionsCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand BackToCoursesCommand { get; }

        public AdminLessonViewModel(
            LessonService lessonService,
            CourseService courseService,
            StorageService storageService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
            _storageService = storageService;

            Lessons = new ObservableCollection<LessonModel>();

            RefreshCommand = new Command(async () => await LoadLessonsForCourseAsync());
            CreateLessonCommand = new Command(CreateNewLesson);
            SaveLessonCommand = new Command(async () => await SaveLessonAsync());
            CancelEditCommand = new Command(CancelEdit);
            DeleteLessonCommand = new Command<LessonModel>(async (lesson) => await DeleteLessonAsync(lesson));
            PickImageCommand = new Command(async () => await PickImageAsync());
            ManageQuestionsCommand = new Command<LessonModel>(async (lesson) => await ManageQuestionsAsync(lesson));
            MoveUpCommand = new Command<LessonModel>(async (lesson) => await MoveUpAsync(lesson));
            MoveDownCommand = new Command<LessonModel>(async (lesson) => await MoveDownAsync(lesson));
            BackToCoursesCommand = new Command(async () => await NavigateBackToCoursesAsync());
        }

        // Tải khóa học và danh sách bài học
        public async Task LoadCourseAndLessonsAsync()
        {
            if (string.IsNullOrEmpty(CourseId))
            {
                ErrorMessage = "Thiếu ID khóa học.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                CurrentCourse = await _courseService.GetCourseByIdAsync(CourseId, idToken);
                if (CurrentCourse == null)
                {
                    ErrorMessage = "Không tìm thấy khóa học.";
                    return;
                }

                await LoadLessonsForCourseAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải khóa học: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Tải danh sách bài học cho khóa học
        private async Task LoadLessonsForCourseAsync()
        {
            if (string.IsNullOrEmpty(CourseId))
            {
                ErrorMessage = "Thiếu ID khóa học.";
                return;
            }

            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                // Lấy danh sách bài học theo CourseId
                var courseLessons = await _lessonService.GetLessonsByCourseIdAsync(CourseId, idToken);
                if (courseLessons == null || courseLessons.Count == 0)
                {
                    ErrorMessage = "Không tìm thấy bài học nào cho khóa học này.";
                    Lessons.Clear();
                    return;
                }

                // Ghi log số lượng bài học đã tải
                Console.WriteLine($"Đã tải {courseLessons.Count} bài học cho CourseId: {CourseId}");

                // Sắp xếp bài học theo thứ tự (Order)
                courseLessons.Sort((a, b) => a.Order.CompareTo(b.Order));

                // Cập nhật danh sách bài học
                Lessons.Clear();
                foreach (var lesson in courseLessons)
                {
                    Lessons.Add(lesson);
                    Console.WriteLine($"Đã thêm bài học vào giao diện: {lesson.Title} (Thứ tự: {lesson.Order})");
                }

                // Cập nhật số lượng bài học trong khóa học
                if (CurrentCourse != null && CurrentCourse.TotalLessons != courseLessons.Count)
                {
                    CurrentCourse.TotalLessons = courseLessons.Count;
                    await _courseService.UpdateCourseAsync(CurrentCourse, idToken);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải bài học cho khóa học ID {CourseId}: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        // Tạo bài học mới
        private void CreateNewLesson()
        {
            LessonId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Content = string.Empty;
            ImageUrl = string.Empty;
            VideoUrl = string.Empty;
            Order = Lessons.Count + 1;
            RequiredPoints = 0;
            MaxPoints = 100;
            EstimatedMinutes = 10;

            IsNewLesson = true;
            IsEditing = true;
            SelectedLesson = null;
            ErrorMessage = string.Empty;
        }

        // Lưu bài học
        private async Task SaveLessonAsync()
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

            if (string.IsNullOrWhiteSpace(Content))
            {
                ErrorMessage = "Nội dung là bắt buộc.";
                return;
            }

            if (Order <= 0)
            {
                ErrorMessage = "Thứ tự phải lớn hơn 0.";
                return;
            }

            if (RequiredPoints < 0)
            {
                ErrorMessage = "Điểm yêu cầu không được âm.";
                return;
            }

            if (MaxPoints <= 0)
            {
                ErrorMessage = "Điểm tối đa phải lớn hơn 0.";
                return;
            }

            if (EstimatedMinutes <= 0)
            {
                ErrorMessage = "Thời gian ước tính phải lớn hơn 0.";
                return;
            }

            if (!string.IsNullOrEmpty(VideoUrl) && !IsValidUrl(VideoUrl))
            {
                ErrorMessage = "URL video không hợp lệ. Vui lòng nhập URL YouTube hoặc Vimeo hợp lệ.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                if (IsNewLesson)
                {
                    var newLesson = new LessonModel
                    {
                        CourseId = CourseId,
                        Title = Title,
                        Description = Description,
                        Content = Content,
                        ImageUrl = ImageUrl,
                        VideoUrl = VideoUrl,
                        Order = Order,
                        RequiredPointsToUnlock = RequiredPoints,
                        MaxPoints = MaxPoints,
                        EstimatedMinutes = EstimatedMinutes
                    };

                    var lessonId = await _lessonService.CreateLessonAsync(newLesson, idToken);
                    if (!string.IsNullOrEmpty(lessonId))
                    {
                        newLesson.LessonId = lessonId;
                        Console.WriteLine($"Đã tạo bài học mới với ID: {lessonId}, Tiêu đề: {newLesson.Title}");

                        // Thêm bài học mới vào danh sách ngay lập tức
                        Lessons.Add(newLesson);
                        Console.WriteLine($"Đã thêm bài học mới vào giao diện: {newLesson.Title} (Thứ tự: {newLesson.Order})");

                        // Cập nhật số lượng bài học trong khóa học
                        if (CurrentCourse != null)
                        {
                            CurrentCourse.TotalLessons++;
                            await _courseService.UpdateCourseAsync(CurrentCourse, idToken);
                            Console.WriteLine($"Đã cập nhật TotalLessons cho CourseId {CourseId}: {CurrentCourse.TotalLessons}");
                        }

                        CancelEdit();

                        // Làm mới danh sách để đảm bảo đồng bộ với backend
                        await LoadLessonsForCourseAsync();
                    }
                    else
                    {
                        ErrorMessage = "Không thể tạo bài học.";
                        await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
                    }
                }
                else
                {
                    if (_selectedLesson != null)
                    {
                        _selectedLesson.Title = Title;
                        _selectedLesson.Description = Description;
                        _selectedLesson.Content = Content;
                        _selectedLesson.ImageUrl = ImageUrl;
                        _selectedLesson.VideoUrl = VideoUrl;
                        _selectedLesson.Order = Order;
                        _selectedLesson.RequiredPointsToUnlock = RequiredPoints;
                        _selectedLesson.MaxPoints = MaxPoints;
                        _selectedLesson.EstimatedMinutes = EstimatedMinutes;

                        var success = await _lessonService.UpdateLessonAsync(_selectedLesson, idToken);
                        if (success)
                        {
                            Console.WriteLine($"Đã cập nhật bài học: {_selectedLesson.Title}");
                            CancelEdit();
                            await LoadLessonsForCourseAsync();
                        }
                        else
                        {
                            ErrorMessage = "Không thể cập nhật bài học.";
                            await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi lưu bài học: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Hủy chỉnh sửa
        private void CancelEdit()
        {
            IsEditing = false;
            IsNewLesson = false;
            SelectedLesson = null;

            LessonId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Content = string.Empty;
            ImageUrl = string.Empty;
            VideoUrl = string.Empty;
            Order = 0;
            RequiredPoints = 0;
            MaxPoints = 0;
            EstimatedMinutes = 0;

            ErrorMessage = string.Empty;
        }

        // Xóa bài học
        private async Task DeleteLessonAsync(LessonModel lesson)
        {
            if (lesson == null)
                return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Xác nhận xóa",
                $"Bạn có chắc chắn muốn xóa bài học '{lesson.Title}'? Thao tác này cũng sẽ xóa tất cả câu hỏi liên quan.",
                "Có", "Không");

            if (!confirm)
                return;

            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                if (!string.IsNullOrEmpty(lesson.ImageUrl))
                {
                    await _storageService.DeleteImageAsync(lesson.ImageUrl, idToken);
                }

                var success = await _lessonService.DeleteLessonAsync(lesson.LessonId, idToken);
                if (success)
                {
                    Lessons.Remove(lesson);
                    if (CurrentCourse != null)
                    {
                        CurrentCourse.TotalLessons--;
                        await _courseService.UpdateCourseAsync(CurrentCourse, idToken);
                    }
                    await ReorderLessonsAsync();
                    if (SelectedLesson == lesson)
                    {
                        CancelEdit();
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể xóa bài học.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi xóa bài học: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Chọn hình ảnh
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
                    PickerTitle = "Chọn hình ảnh bài học"
                });

                if (result != null)
                {
                    IsLoading = true;
                    var idToken = LocalStorageHelper.GetItem("idToken");
                    if (string.IsNullOrEmpty(idToken))
                    {
                        await App.Current.MainPage.DisplayAlert("Lỗi", "Người dùng chưa được xác thực. Vui lòng đăng nhập lại.", "OK");
                        return;
                    }

                    var imageUrl = await _storageService.UploadImageFromPickerAsync(result, "lesson_images", idToken);
                    if (!string.IsNullOrEmpty(imageUrl))
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
                IsLoading = false;
            }
        }

        // Quản lý câu hỏi
        private async Task ManageQuestionsAsync(LessonModel lesson)
        {
            if (lesson == null)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Không có bài học được chọn.", "OK");
                return;
            }

            try
            {
                await Shell.Current.GoToAsync($"questionManagement?lessonId={lesson.LessonId}&courseId={CourseId}");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể điều hướng đến quản lý câu hỏi: {ex.Message}", "OK");
            }
        }

        // Di chuyển bài học lên
        private async Task MoveUpAsync(LessonModel lesson)
        {
            if (lesson == null)
                return;

            var index = Lessons.IndexOf(lesson);
            if (index <= 0)
                return;

            IsLoading = true;
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                var previousLesson = Lessons[index - 1];
                int tempOrder = lesson.Order;
                lesson.Order = previousLesson.Order;
                previousLesson.Order = tempOrder;

                Lessons.Move(index, index - 1);

                await Task.WhenAll(
                    _lessonService.UpdateLessonAsync(lesson, idToken),
                    _lessonService.UpdateLessonAsync(previousLesson, idToken)
                );
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi di chuyển bài học lên: {ex.Message}", "OK");
                await LoadLessonsForCourseAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Di chuyển bài học xuống
        private async Task MoveDownAsync(LessonModel lesson)
        {
            if (lesson == null)
                return;

            var index = Lessons.IndexOf(lesson);
            if (index >= Lessons.Count - 1)
                return;

            IsLoading = true;
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                var nextLesson = Lessons[index + 1];
                int tempOrder = lesson.Order;
                lesson.Order = nextLesson.Order;
                nextLesson.Order = tempOrder;

                Lessons.Move(index, index + 1);

                await Task.WhenAll(
                    _lessonService.UpdateLessonAsync(lesson, idToken),
                    _lessonService.UpdateLessonAsync(nextLesson, idToken)
                );
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi di chuyển bài học xuống: {ex.Message}", "OK");
                await LoadLessonsForCourseAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Sắp xếp lại thứ tự bài học
        private async Task ReorderLessonsAsync()
        {
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                var updateTasks = new List<Task>();
                for (int i = 0; i < Lessons.Count; i++)
                {
                    var lesson = Lessons[i];
                    if (lesson.Order != i + 1)
                    {
                        lesson.Order = i + 1;
                        updateTasks.Add(_lessonService.UpdateLessonAsync(lesson, idToken));
                    }
                }
                await Task.WhenAll(updateTasks);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi sắp xếp lại bài học: {ex.Message}", "OK");
            }
        }

        // Quay lại danh sách khóa học
        private async Task NavigateBackToCoursesAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể quay lại: {ex.Message}", "OK");
            }
        }

        // Kiểm tra URL hợp lệ
        private bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
                return false;

            var host = uriResult.Host.ToLower();
            return host.Contains("youtube.com") || host.Contains("video.com");
        }

        // Kiểm tra kết nối mạng
        private async Task<bool> CheckNetworkConnectionAsync()
        {
            var current = Connectivity.NetworkAccess;
            return current == NetworkAccess.Internet;
        }
    }
}