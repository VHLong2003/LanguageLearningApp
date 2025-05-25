using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.Admin
{
    public class UserProgressViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly ProgressService _progressService;
        private readonly BadgeService _badgeService;
        private readonly CourseService _courseService;
        private readonly LessonService _lessonService;

        private string _userId;
        private UsersModel _user;
        private int _badgeCount;
        private ObservableCollection<CourseProgressModel> _courseProgress;
        private ObservableCollection<ActivityModel> _recentActivity;
        private bool _isLoading;
        private bool _isRefreshing;

        public UsersModel User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public int BadgeCount
        {
            get => _badgeCount;
            set => SetProperty(ref _badgeCount, value);
        }

        public ObservableCollection<CourseProgressModel> CourseProgress
        {
            get => _courseProgress;
            set => SetProperty(ref _courseProgress, value);
        }

        public ObservableCollection<ActivityModel> RecentActivity
        {
            get => _recentActivity;
            set => SetProperty(ref _recentActivity, value);
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

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        public UserProgressViewModel(
            UserService userService,
            ProgressService progressService,
            BadgeService badgeService,
            CourseService courseService,
            LessonService lessonService)
        {
            _userService = userService;
            _progressService = progressService;
            _badgeService = badgeService;
            _courseService = courseService;
            _lessonService = lessonService;

            CourseProgress = new ObservableCollection<CourseProgressModel>();
            RecentActivity = new ObservableCollection<ActivityModel>();

            RefreshCommand = new Command(async () => await LoadUserDataAsync());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task InitializeAsync(string userId)
        {
            _userId = userId;
            await LoadUserDataAsync();
        }

        private async Task LoadUserDataAsync()
        {
            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Tải dữ liệu người dùng
                User = await _userService.GetUserByIdAsync(_userId, idToken);

                if (User != null)
                {
                    // Tải số lượng huy hiệu
                    var badges = await _badgeService.GetUserBadgesAsync(_userId, idToken);
                    BadgeCount = badges.Count;

                    // Tải dữ liệu tiến trình
                    var progressData = await _progressService.GetUserProgressAsync(_userId, idToken);

                    // Lấy tất cả khóa học
                    var allCourses = await _courseService.GetAllCoursesAsync(idToken);

                    // Nhóm tiến trình theo khóa học
                    var courseGroups = progressData
                        .GroupBy(p => p.CourseId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    // Xóa và xây dựng lại tiến trình khóa học
                    CourseProgress.Clear();

                    foreach (var courseId in courseGroups.Keys)
                    {
                        var course = allCourses.FirstOrDefault(c => c.CourseId == courseId);
                        if (course != null)
                        {
                            // Lấy các bài học cho khóa học này
                            var courseLessons = await _lessonService.GetLessonsByCourseIdAsync(courseId, idToken);

                            // Tính phần trăm hoàn thành khóa học
                            int completedLessons = courseGroups[courseId].Count(p => p.PercentComplete == 100);
                            double progressPercent = course.TotalLessons > 0 ?
                                (double)completedLessons / course.TotalLessons :
                                0;

                            // Tính tổng số điểm kiếm được trong khóa học này
                            int totalPoints = courseGroups[courseId].Sum(p => p.EarnedPoints);

                            // Tính tổng thời gian sử dụng trong khóa học này (tính bằng giây)
                            int totalTime = courseGroups[courseId].Sum(p => p.TimeSpent);

                            // Tạo dữ liệu tiến trình bài học
                            var lessonProgress = new ObservableCollection<LessonProgressItem>();
                            foreach (var lesson in courseLessons.OrderBy(l => l.Order))
                            {
                                var progress = courseGroups[courseId].FirstOrDefault(p => p.LessonId == lesson.LessonId);

                                lessonProgress.Add(new LessonProgressItem
                                {
                                    LessonId = lesson.LessonId,
                                    LessonTitle = lesson.Title,
                                    IsCompleted = progress != null && progress.PercentComplete == 100,
                                    EarnedPoints = progress?.EarnedPoints ?? 0
                                });
                            }

                            CourseProgress.Add(new CourseProgressModel
                            {
                                Course = course,
                                ProgressPercent = progressPercent,
                                CompletedLessons = completedLessons,
                                TotalLessons = course.TotalLessons,
                                TotalPoints = totalPoints,
                                TotalTime = totalTime,
                                LessonProgress = lessonProgress,
                                ShowDetails = false
                            });
                        }
                    }

                    // Tạo dòng thời gian hoạt động gần đây
                    RecentActivity.Clear();
                    foreach (var progress in progressData.OrderByDescending(p => p.CompletedDate).Take(10))
                    {
                        // Lấy thông tin khóa học và bài học
                        var course = allCourses.FirstOrDefault(c => c.CourseId == progress.CourseId);
                        var lesson = await _lessonService.GetLessonByIdAsync(progress.LessonId, idToken);

                        if (course != null && lesson != null)
                        {
                            RecentActivity.Add(new ActivityModel
                            {
                                Date = progress.CompletedDate,
                                Description = $"Hoàn thành bài học \"{lesson.Title}\" trong \"{course.Title}\"",
                                Points = progress.EarnedPoints
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải dữ liệu người dùng: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }
    }

    public class CourseProgressModel : BaseViewModel
    {
        private CourseModel _course;
        private double _progressPercent;
        private int _completedLessons;
        private int _totalLessons;
        private int _totalPoints;
        private int _totalTime;
        private ObservableCollection<LessonProgressItem> _lessonProgress;
        private bool _showDetails;

        public CourseModel Course
        {
            get => _course;
            set => SetProperty(ref _course, value);
        }

        public double ProgressPercent
        {
            get => _progressPercent;
            set => SetProperty(ref _progressPercent, value);
        }

        public int CompletedLessons
        {
            get => _completedLessons;
            set => SetProperty(ref _completedLessons, value);
        }

        public int TotalLessons
        {
            get => _totalLessons;
            set => SetProperty(ref _totalLessons, value);
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

        public ObservableCollection<LessonProgressItem> LessonProgress
        {
            get => _lessonProgress;
            set => SetProperty(ref _lessonProgress, value);
        }

        public bool ShowDetails
        {
            get => _showDetails;
            set => SetProperty(ref _showDetails, value);
        }

        public ICommand ToggleDetailsCommand { get; }

        public CourseProgressModel()
        {
            LessonProgress = new ObservableCollection<LessonProgressItem>();
            ToggleDetailsCommand = new Command(() => ShowDetails = !ShowDetails);
        }
    }

    public class LessonProgressItem
    {
        public string LessonId { get; set; }
        public string LessonTitle { get; set; }
        public bool IsCompleted { get; set; }
        public int EarnedPoints { get; set; }
    }

    public class ActivityModel
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public int Points { get; set; }
    }
}