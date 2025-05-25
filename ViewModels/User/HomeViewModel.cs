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
    public class HomeViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly CourseService _courseService;
        private readonly LessonService _lessonService;
        private readonly ProgressService _progressService;
        private readonly LeaderboardService _leaderboardService;
        private readonly BadgeService _badgeService;

        private string _welcomeMessage;
        private string _dailyMotivation;
        private int _currentStreak;
        private int _totalPoints;
        private double _dailyGoalProgress;
        private string _dailyGoalMessage;
        private bool _hasCurrentCourse;
        private CourseModel _currentCourse;
        private string _currentLessonTitle;
        private double _courseProgress;
        private string _courseProgressText;
        private ObservableCollection<CourseModel> _recommendedCourses;
        private ObservableCollection<LeaderboardModel> _topLeaderboard;
        private ObservableCollection<BadgeModel> _recentBadges;
        private bool _hasRecentBadges;
        private bool _isRefreshing;

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public string DailyMotivation
        {
            get => _dailyMotivation;
            set => SetProperty(ref _dailyMotivation, value);
        }

        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }

        public int TotalPoints
        {
            get => _totalPoints;
            set => SetProperty(ref _totalPoints, value);
        }

        public double DailyGoalProgress
        {
            get => _dailyGoalProgress;
            set => SetProperty(ref _dailyGoalProgress, value);
        }

        public string DailyGoalMessage
        {
            get => _dailyGoalMessage;
            set => SetProperty(ref _dailyGoalMessage, value);
        }

        public bool HasCurrentCourse
        {
            get => _hasCurrentCourse;
            set => SetProperty(ref _hasCurrentCourse, value);
        }

        public CourseModel CurrentCourse
        {
            get => _currentCourse;
            set => SetProperty(ref _currentCourse, value);
        }

        public string CurrentLessonTitle
        {
            get => _currentLessonTitle;
            set => SetProperty(ref _currentLessonTitle, value);
        }

        public double CourseProgress
        {
            get => _courseProgress;
            set => SetProperty(ref _courseProgress, value);
        }

        public string CourseProgressText
        {
            get => _courseProgressText;
            set => SetProperty(ref _courseProgressText, value);
        }

        public ObservableCollection<CourseModel> RecommendedCourses
        {
            get => _recommendedCourses;
            set => SetProperty(ref _recommendedCourses, value);
        }

        public ObservableCollection<LeaderboardModel> TopLeaderboard
        {
            get => _topLeaderboard;
            set => SetProperty(ref _topLeaderboard, value);
        }

        public ObservableCollection<BadgeModel> RecentBadges
        {
            get => _recentBadges;
            set => SetProperty(ref _recentBadges, value);
        }

        public bool HasRecentBadges
        {
            get => _hasRecentBadges;
            set => SetProperty(ref _hasRecentBadges, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        // Các lệnh
        public ICommand RefreshCommand { get; }
        public ICommand ContinueLearningCommand { get; }
        public ICommand CourseSelectedCommand { get; }
        public ICommand ViewLeaderboardCommand { get; }

        // Các câu động lực hàng ngày
        private readonly string[] _motivations = new string[]
        {
            "Tính kiên trì là chìa khóa để thành thạo một kỹ năng mới!",
            "Mỗi bài học hoàn thành là một bước tiến tới sự thông thạo.",
            "Học một chút mỗi ngày sẽ dẫn đến những kết quả lớn.",
            "Bộ não của bạn giống như một cơ bắp - hãy rèn luyện nó hàng ngày!",
            "Càng luyện tập nhiều, mọi thứ sẽ càng trở nên dễ dàng.",
            "Những bước nhỏ mỗi ngày dẫn đến những thành tựu lớn.",
            "Tương lai của bạn sẽ cảm ơn bạn vì đã học hôm nay.",
            "Thời điểm tốt nhất để học luôn là bây giờ.",
            "Mọi chuyên gia đều từng là người mới bắt đầu.",
            "Hãy giữ chuỗi học tập của bạn - bạn đang làm rất tốt!"
        };

        public HomeViewModel(
            UserService userService,
            CourseService courseService,
            LessonService lessonService,
            ProgressService progressService,
            LeaderboardService leaderboardService,
            BadgeService badgeService)
        {
            _userService = userService;
            _courseService = courseService;
            _lessonService = lessonService;
            _progressService = progressService;
            _leaderboardService = leaderboardService;
            _badgeService = badgeService;

            RecommendedCourses = new ObservableCollection<CourseModel>();
            TopLeaderboard = new ObservableCollection<LeaderboardModel>();
            RecentBadges = new ObservableCollection<BadgeModel>();

            RefreshCommand = new Command(async () => await LoadHomeDataAsync());
            ContinueLearningCommand = new Command(async () => await ContinueLearningAsync());
            CourseSelectedCommand = new Command<CourseModel>(async (course) => await OnCourseSelected(course));
            ViewLeaderboardCommand = new Command(async () => await ViewLeaderboardAsync());

            // Thiết lập giá trị mặc định
            DailyGoalProgress = 0;
            HasCurrentCourse = false;
            HasRecentBadges = false;
        }

        public async Task InitializeAsync()
        {
            await LoadHomeDataAsync();
        }

        private async Task LoadHomeDataAsync()
        {
            IsRefreshing = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Tải dữ liệu người dùng
                var user = await _userService.GetUserByIdAsync(userId, idToken);

                if (user != null)
                {
                    // Thiết lập thông điệp chào hỏi dựa trên thời gian trong ngày
                    var timeOfDay = DateTime.Now.Hour;
                    string greeting;

                    if (timeOfDay < 12)
                        greeting = "Chào buổi sáng";
                    else if (timeOfDay < 18)
                        greeting = "Chào buổi chiều";
                    else
                        greeting = "Chào buổi tối";

                    WelcomeMessage = $"{greeting}, {user.FullName}!";

                    // Thiết lập câu động lực ngẫu nhiên hàng ngày
                    var random = new Random();
                    DailyMotivation = _motivations[random.Next(_motivations.Length)];

                    // Thiết lập thống kê người dùng
                    CurrentStreak = user.CurrentStreak;
                    TotalPoints = user.Points;

                    // Thiết lập tiến trình mục tiêu hàng ngày
                    // Giả sử mục tiêu hàng ngày là 5 bài học hoặc 50 điểm
                    var today = DateTime.Today;
                    var todayProgress = await _progressService.GetUserProgressAsync(userId, idToken);
                    var todayCompletions = todayProgress.Count(p => p.CompletedDate.Date == today);
                    var todayPoints = todayProgress.Where(p => p.CompletedDate.Date == today).Sum(p => p.EarnedPoints);

                    // Sử dụng giá trị lớn hơn - số bài học hoặc điểm
                    var lessonProgress = todayCompletions / 5.0; // 5 bài học mỗi ngày
                    var pointsProgress = todayPoints / 50.0; // 50 điểm mỗi ngày
                    DailyGoalProgress = Math.Min(1.0, Math.Max(lessonProgress, pointsProgress));

                    if (DailyGoalProgress >= 1.0)
                        DailyGoalMessage = "Hoàn thành mục tiêu hàng ngày! Tuyệt vời!";
                    else if (DailyGoalProgress > 0)
                        DailyGoalMessage = $"Tiến trình: {DailyGoalProgress:P0} hướng tới mục tiêu hàng ngày";
                    else
                        DailyGoalMessage = "Bắt đầu học để đạt mục tiêu hàng ngày!";

                    // Tải các huy hiệu gần đây
                    var badges = await _badgeService.GetUserBadgesAsync(userId, idToken);
                    RecentBadges.Clear();

                    if (badges.Count > 0)
                    {
                        HasRecentBadges = true;
                        foreach (var badge in badges.Take(5)) // Lấy 5 huy hiệu gần đây nhất
                        {
                            RecentBadges.Add(badge);
                        }
                    }
                    else
                    {
                        HasRecentBadges = false;
                    }

                    // Tìm khóa học hiện tại đang tiến hành
                    var allProgress = await _progressService.GetUserProgressAsync(userId, idToken);

                    if (allProgress.Count > 0)
                    {
                        // Nhóm theo khóa học và tìm khóa học gần đây nhất
                        var courseGroups = allProgress
                            .GroupBy(p => p.CourseId)
                            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.LastAttemptDate).First());

                        if (courseGroups.Any())
                        {
                            var mostRecentProgress = courseGroups.Values.OrderByDescending(p => p.LastAttemptDate).First();
                            var course = await _courseService.GetCourseByIdAsync(mostRecentProgress.CourseId, idToken);

                            if (course != null)
                            {
                                CurrentCourse = course;
                                HasCurrentCourse = true;

                                // Tính tiến trình khóa học
                                CourseProgress = await _progressService.GetCourseProgressPercentAsync(userId, course.CourseId, idToken) / 100.0;
                                CourseProgressText = $"Hoàn thành {CourseProgress:P0}";

                                // Tìm bài học hiện tại
                                var lessons = await _lessonService.GetLessonsByCourseIdAsync(course.CourseId, idToken);

                                if (lessons.Count > 0)
                                {
                                    // Sắp xếp theo thứ tự
                                    lessons.Sort((a, b) => a.Order.CompareTo(b.Order));

                                    // Tìm bài học đầu tiên chưa hoàn thành 100%
                                    var currentLesson = lessons.FirstOrDefault(l =>
                                    {
                                        var lessonProgress = allProgress.FirstOrDefault(p => p.LessonId == l.LessonId);
                                        return lessonProgress == null || lessonProgress.PercentComplete < 100;
                                    });

                                    if (currentLesson != null)
                                    {
                                        CurrentLessonTitle = currentLesson.Title;
                                    }
                                    else
                                    {
                                        // Tất cả bài học đã hoàn thành
                                        CurrentLessonTitle = "Tất cả bài học đã hoàn thành!";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        HasCurrentCourse = false;
                    }

                    // Tải các khóa học được đề xuất
                    var allCourses = await _courseService.GetPublishedCoursesAsync(idToken);
                    var userLevel = user.Points / 100; // Ước tính cấp độ người dùng

                    // Lọc các khóa học dựa trên cấp độ người dùng và chưa hoàn thành
                    var completedCourseIds = allProgress
                        .GroupBy(p => p.CourseId)
                        .Where(g => g.Count() >= 5) // Giả sử hoàn thành ít nhất 5 bài học nghĩa là khóa học đã xong
                        .Select(g => g.Key)
                        .ToList();

                    var recommendedCourses = allCourses
                        .Where(c => !completedCourseIds.Contains(c.CourseId) &&
                               c.RequiredPointsToUnlock <= user.Points &&
                               c.DifficultyLevel <= userLevel + 2) // Hơi thử thách
                        .OrderBy(c => c.DifficultyLevel)
                        .Take(5)
                        .ToList();

                    RecommendedCourses.Clear();
                    foreach (var course in recommendedCourses)
                    {
                        RecommendedCourses.Add(course);
                    }

                    // Tải bảng xếp hạng
                    var leaderboard = await _leaderboardService.GetGlobalLeaderboardAsync(idToken);
                    var topUsers = leaderboard.Take(5).ToList();

                    TopLeaderboard.Clear();
                    foreach (var entry in topUsers)
                    {
                        TopLeaderboard.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải dữ liệu trang chủ: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task ContinueLearningAsync()
        {
            if (CurrentCourse == null)
                return;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Lấy tất cả bài học cho khóa học
                var lessons = await _lessonService.GetLessonsByCourseIdAsync(CurrentCourse.CourseId, idToken);

                if (lessons.Count > 0)
                {
                    // Sắp xếp theo thứ tự
                    lessons.Sort((a, b) => a.Order.CompareTo(b.Order));

                    // Lấy tất cả tiến trình người dùng cho khóa học này
                    var allProgress = await _progressService.GetUserProgressAsync(userId, idToken);

                    // Tìm bài học đầu tiên chưa hoàn thành
                    var currentLesson = lessons.FirstOrDefault(l =>
                    {
                        var lessonProgress = allProgress.FirstOrDefault(p => p.LessonId == l.LessonId);
                        return lessonProgress == null || lessonProgress.PercentComplete < 100;
                    });

                    if (currentLesson != null)
                    {
                        // Chuyển đến bài học
                        await Shell.Current.GoToAsync($"lesson?lessonId={currentLesson.LessonId}&courseId={CurrentCourse.CourseId}");
                    }
                    else
                    {
                        // Tất cả bài học đã hoàn thành, chuyển đến chi tiết khóa học
                        await Shell.Current.GoToAsync($"courseDetail?courseId={CurrentCourse.CourseId}");
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tiếp tục học: {ex.Message}", "OK");
            }
        }

        private async Task OnCourseSelected(CourseModel course)
        {
            if (course == null)
                return;

            // Chuyển đến chi tiết khóa học
            await Shell.Current.GoToAsync($"courseDetail?courseId={course.CourseId}");
        }

        private async Task ViewLeaderboardAsync()
        {
            // Chuyển đến tab bảng xếp hạng
            await Shell.Current.GoToAsync("//leaderboard");
        }
    }
}