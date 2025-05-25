using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.User
{
    [QueryProperty(nameof(CourseId), "courseId")]
    public class CourseDetailViewModel : BaseViewModel
    {
        private readonly CourseService _courseService;
        private readonly LessonService _lessonService;
        private readonly ProgressService _progressService;
        private readonly UserService _userService;
        private readonly LessonProgressService _lessonProgressService;

        private string _courseId;
        private CourseModel _course;
        private ObservableCollection<LessonProgressItem> _lessonsWithProgress;
        private double _courseProgress;
        private bool _isLoading;
        private bool _isRefreshing;
        private string _errorMessage;
        private UsersModel _currentUser;
        private int _completedLessons;
        private int _totalPoints;
        private int _totalTime;
        private string _nextLessonTitle;

        public string CourseId
        {
            get => _courseId;
            set
            {
                if (SetProperty(ref _courseId, value) && !string.IsNullOrEmpty(value))
                {
                    // Khi courseId được đặt qua QueryProperty, tải dữ liệu
                    Task.Run(async () => await LoadCourseDetailsAsync());
                }
            }
        }

        public CourseModel Course
        {
            get => _course;
            set => SetProperty(ref _course, value);
        }

        public ObservableCollection<LessonProgressItem> LessonsWithProgress
        {
            get => _lessonsWithProgress;
            set => SetProperty(ref _lessonsWithProgress, value);
        }

        public double CourseProgress
        {
            get => _courseProgress;
            set => SetProperty(ref _courseProgress, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public int CompletedLessons
        {
            get => _completedLessons;
            set => SetProperty(ref _completedLessons, value);
        }

        public int TotalPoints
        {
            get => _totalPoints;
            set => SetProperty(ref _totalPoints, value);
        }

        public int TotalTime
        {
            get => _totalTime;
            set => SetProperty(ref _totalTime, value);
        }

        public string NextLessonTitle
        {
            get => _nextLessonTitle;
            set => SetProperty(ref _nextLessonTitle, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand LessonSelectedCommand { get; }
        public ICommand ShareCourseCommand { get; }

        public CourseDetailViewModel(
            CourseService courseService,
            LessonService lessonService,
            ProgressService progressService,
            UserService userService,
            LessonProgressService lessonProgressService)
        {
            _courseService = courseService;
            _lessonService = lessonService;
            _progressService = progressService;
            _userService = userService;
            _lessonProgressService = lessonProgressService;

            LessonsWithProgress = new ObservableCollection<LessonProgressItem>();

            RefreshCommand = new Command(async () => await RefreshLessonsAsync());
            LessonSelectedCommand = new Command<LessonProgressItem>(async (lesson) => await LessonSelectedAsync(lesson));
            ShareCourseCommand = new Command(async () => await ShareCourseAsync());
        }

        public async Task InitializeAsync(string courseId)
        {
            CourseId = courseId;
            await LoadCourseDetailsAsync();
        }

        private async Task LoadCourseDetailsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Tải dữ liệu người dùng hiện tại
                _currentUser = await _userService.GetUserByIdAsync(userId, idToken);

                // Tải chi tiết khóa học
                Course = await _courseService.GetCourseByIdAsync(CourseId, idToken);

                if (Course != null)
                {
                    // Tải bài học và tiến trình
                    await LoadLessonsAndProgressAsync();

                    // Cập nhật tiến trình khóa học
                    CourseProgress = await _lessonProgressService.GetOverallCourseProgressAsync(userId, CourseId, idToken) / 100.0;
                }
                else
                {
                    ErrorMessage = "Không tìm thấy khóa học";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải khóa học: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshLessonsAsync()
        {
            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                await LoadLessonsAndProgressAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi làm mới bài học: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task LoadLessonsAndProgressAsync()
        {
            var idToken = LocalStorageHelper.GetItem("idToken");
            var userId = LocalStorageHelper.GetItem("userId");

            // Tải tất cả bài học cho khóa học này
            var lessons = await _lessonService.GetLessonsByCourseIdAsync(CourseId, idToken);

            // Sắp xếp theo thứ tự
            lessons.Sort((a, b) => a.Order.CompareTo(b.Order));

            // Lấy thông tin tiến trình cho tất cả bài học trong khóa học này
            var lessonProgressList = await _lessonProgressService.GetUserLessonProgressAsync(userId, CourseId, idToken);

            // Lấy các bản ghi tiến trình cũ để đảm bảo tính tương thích ngược và thống kê
            var progressList = await _progressService.GetUserProgressAsync(userId, idToken);
            var courseProgress = progressList.Where(p => p.CourseId == CourseId).ToList();

            // Theo dõi thống kê khóa học
            CompletedLessons = 0;
            TotalPoints = 0;
            TotalTime = 0;

            LessonsWithProgress.Clear();
            int unlocked = 0;
            bool foundNextLesson = false;
            int previousLessonComplete = 1; // Bài học đầu tiên luôn được mở khóa

            foreach (var lesson in lessons)
            {
                var lessonProgress = lessonProgressList.FirstOrDefault(p => p.LessonId == lesson.LessonId);
                var oldProgress = courseProgress.FirstOrDefault(p => p.LessonId == lesson.LessonId);

                bool isCompleted = lessonProgress?.IsCompleted ?? false;
                double percentComplete = lessonProgress?.PercentComplete ?? 0;
                int earnedPoints = lessonProgress?.EarnedPoints ?? 0;

                // Sử dụng mô hình tiến trình cũ nếu cần
                if (!isCompleted && oldProgress != null && oldProgress.PercentComplete == 100)
                {
                    isCompleted = true;
                    percentComplete = 100;
                    earnedPoints = oldProgress.EarnedPoints;
                }

                bool isUnlocked = lesson.Order == 1 || // Bài học đầu tiên luôn được mở khóa
                                (_currentUser.Points >= lesson.RequiredPointsToUnlock && previousLessonComplete == 1);

                // Cập nhật thống kê khóa học
                if (isCompleted)
                {
                    CompletedLessons++;
                    TotalPoints += earnedPoints;
                    TotalTime += lessonProgress?.TimeSpent ?? 0;
                }

                if (isUnlocked)
                    unlocked++;

                previousLessonComplete = isCompleted ? 1 : 0;

                // Tìm bài học tiếp theo cần hoàn thành
                if (!isCompleted && isUnlocked && !foundNextLesson)
                {
                    NextLessonTitle = lesson.Title;
                    foundNextLesson = true;
                }

                LessonsWithProgress.Add(new LessonProgressItem
                {
                    Lesson = lesson,
                    IsCompleted = isCompleted,
                    IsUnlocked = isUnlocked,
                    ProgressPercent = percentComplete,
                    EarnedPoints = earnedPoints,
                    TimeSpent = lessonProgress?.TimeSpent ?? 0,
                    CurrentQuestionIndex = lessonProgress?.CurrentQuestionIndex ?? 0,
                    TotalQuestions = lessonProgress?.TotalQuestions ?? 0
                });
            }

            // Nếu tất cả bài học đã hoàn thành
            if (!foundNextLesson)
            {
                NextLessonTitle = "Tất cả bài học đã hoàn thành!";
            }
        }

        private async Task LessonSelectedAsync(LessonProgressItem lessonProgress)
        {
            if (lessonProgress == null)
                return;

            if (!lessonProgress.IsUnlocked)
            {
                // Hiển thị thông báo về bài học bị khóa
                await App.Current.MainPage.DisplayAlert(
                    "Bài học bị khóa",
                    $"Bạn cần {lessonProgress.Lesson.RequiredPointsToUnlock} điểm để mở khóa bài học này. Hiện tại bạn có {_currentUser.Points} điểm.",
                    "OK");
                return;
            }

            // Chuyển đến bài học
            await Shell.Current.GoToAsync($"lesson?lessonId={lessonProgress.Lesson.LessonId}&courseId={CourseId}");
        }

        private async Task ShareCourseAsync()
        {
            if (Course == null)
                return;

            try
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Title = $"Hãy xem khóa học này: {Course.Title}",
                    Text = $"Tôi đang học {Course.Title} trên ứng dụng Language Learning App. Đây là khóa học {Course.Type} với {Course.TotalLessons} bài học. Tham gia cùng tôi!"
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Chia sẻ thất bại", $"Không thể chia sẻ khóa học: {ex.Message}", "OK");
            }
        }
    }

    // Lớp hỗ trợ để biểu diễn tiến trình bài học trong giao diện người dùng
    public class LessonProgressItem : BaseViewModel
    {
        private LessonModel _lesson;
        private bool _isCompleted;
        private bool _isUnlocked;
        private double _progressPercent;
        private int _earnedPoints;
        private int _timeSpent;
        private int _currentQuestionIndex;
        private int _totalQuestions;

        public LessonModel Lesson
        {
            get => _lesson;
            set => SetProperty(ref _lesson, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set => SetProperty(ref _isUnlocked, value);
        }

        public double ProgressPercent
        {
            get => _progressPercent;
            set => SetProperty(ref _progressPercent, value);
        }

        public int EarnedPoints
        {
            get => _earnedPoints;
            set => SetProperty(ref _earnedPoints, value);
        }

        public int TimeSpent
        {
            get => _timeSpent;
            set => SetProperty(ref _timeSpent, value);
        }

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set => SetProperty(ref _currentQuestionIndex, value);
        }

        public int TotalQuestions
        {
            get => _totalQuestions;
            set => SetProperty(ref _totalQuestions, value);
        }

        public string Status
        {
            get
            {
                if (IsCompleted)
                    return "Hoàn thành";
                if (!IsUnlocked)
                    return "Bị khóa";
                if (CurrentQuestionIndex > 0)
                    return "Đang tiến hành";
                return "Chưa bắt đầu";
            }
        }

        public string ProgressText
        {
            get
            {
                if (TotalQuestions == 0)
                    return "";

                if (IsCompleted)
                    return "100%";

                if (CurrentQuestionIndex > 0)
                    return $"{Math.Round(ProgressPercent)}%";

                return "0%";
            }
        }
    }
}