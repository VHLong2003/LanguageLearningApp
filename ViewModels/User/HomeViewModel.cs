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

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand ContinueLearningCommand { get; }
        public ICommand CourseSelectedCommand { get; }
        public ICommand ViewLeaderboardCommand { get; }

        // Daily motivations
        private readonly string[] _motivations = new string[]
        {
            "Consistency is key to mastering a new skill!",
            "Every lesson completed is a step towards fluency.",
            "Learning a little each day adds up to big results.",
            "Your brain is like a muscle - exercise it daily!",
            "The more you practice, the easier it becomes.",
            "Small steps every day lead to big achievements.",
            "Your future self will thank you for learning today.",
            "The best time to learn is always now.",
            "Every expert was once a beginner.",
            "Keep your streak going - you're doing great!"
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

            // Set default values
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

                // Load user data
                var user = await _userService.GetUserByIdAsync(userId, idToken);

                if (user != null)
                {
                    // Set welcome message based on time of day
                    var timeOfDay = DateTime.Now.Hour;
                    string greeting;

                    if (timeOfDay < 12)
                        greeting = "Good Morning";
                    else if (timeOfDay < 18)
                        greeting = "Good Afternoon";
                    else
                        greeting = "Good Evening";

                    WelcomeMessage = $"{greeting}, {user.FullName}!";

                    // Set random daily motivation
                    var random = new Random();
                    DailyMotivation = _motivations[random.Next(_motivations.Length)];

                    // Set user stats
                    CurrentStreak = user.CurrentStreak;
                    TotalPoints = user.Points;

                    // Set daily goal progress
                    // Assuming daily goal is 5 lessons or 50 points
                    var today = DateTime.Today;
                    var todayProgress = await _progressService.GetUserProgressAsync(userId, idToken);
                    var todayCompletions = todayProgress.Count(p => p.CompletedDate.Date == today);
                    var todayPoints = todayProgress.Where(p => p.CompletedDate.Date == today).Sum(p => p.EarnedPoints);

                    // Use whichever is greater - lesson count or points
                    var lessonProgress = todayCompletions / 5.0; // 5 lessons per day
                    var pointsProgress = todayPoints / 50.0; // 50 points per day
                    DailyGoalProgress = Math.Min(1.0, Math.Max(lessonProgress, pointsProgress));

                    if (DailyGoalProgress >= 1.0)
                        DailyGoalMessage = "Daily goal complete! Great job!";
                    else if (DailyGoalProgress > 0)
                        DailyGoalMessage = $"Progress: {DailyGoalProgress:P0} toward daily goal";
                    else
                        DailyGoalMessage = "Start learning to reach your daily goal!";

                    // Load recent badges
                    var badges = await _badgeService.GetUserBadgesAsync(userId, idToken);
                    RecentBadges.Clear();

                    if (badges.Count > 0)
                    {
                        HasRecentBadges = true;
                        foreach (var badge in badges.Take(5)) // Take most recent 5
                        {
                            RecentBadges.Add(badge);
                        }
                    }
                    else
                    {
                        HasRecentBadges = false;
                    }

                    // Find current course in progress
                    var allProgress = await _progressService.GetUserProgressAsync(userId, idToken);

                    if (allProgress.Count > 0)
                    {
                        // Group by course and find most recent
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

                                // Calculate course progress
                                CourseProgress = await _progressService.GetCourseProgressPercentAsync(userId, course.CourseId, idToken) / 100.0;
                                CourseProgressText = $"{CourseProgress:P0} Complete";

                                // Find current lesson
                                var lessons = await _lessonService.GetLessonsByCourseIdAsync(course.CourseId, idToken);

                                if (lessons.Count > 0)
                                {
                                    // Sort by order
                                    lessons.Sort((a, b) => a.Order.CompareTo(b.Order));

                                    // Find first lesson that's not 100% complete
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
                                        // All lessons complete
                                        CurrentLessonTitle = "All lessons completed!";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        HasCurrentCourse = false;
                    }

                    // Load recommended courses
                    var allCourses = await _courseService.GetPublishedCoursesAsync(idToken);
                    var userLevel = user.Points / 100; // Estimate user level

                    // Filter courses based on user level and not already completed
                    var completedCourseIds = allProgress
                        .GroupBy(p => p.CourseId)
                        .Where(g => g.Count() >= 5) // Assuming at least 5 lessons completed means course is done
                        .Select(g => g.Key)
                        .ToList();

                    var recommendedCourses = allCourses
                        .Where(c => !completedCourseIds.Contains(c.CourseId) &&
                               c.RequiredPointsToUnlock <= user.Points &&
                               c.DifficultyLevel <= userLevel + 2) // Slightly challenging
                        .OrderBy(c => c.DifficultyLevel)
                        .Take(5)
                        .ToList();

                    RecommendedCourses.Clear();
                    foreach (var course in recommendedCourses)
                    {
                        RecommendedCourses.Add(course);
                    }

                    // Load leaderboard
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
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load home data: {ex.Message}", "OK");
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

                // Get all lessons for the course
                var lessons = await _lessonService.GetLessonsByCourseIdAsync(CurrentCourse.CourseId, idToken);

                if (lessons.Count > 0)
                {
                    // Sort by order
                    lessons.Sort((a, b) => a.Order.CompareTo(b.Order));

                    // Get all user progress for this course
                    var allProgress = await _progressService.GetUserProgressAsync(userId, idToken);

                    // Find first lesson that's not complete
                    var currentLesson = lessons.FirstOrDefault(l =>
                    {
                        var lessonProgress = allProgress.FirstOrDefault(p => p.LessonId == l.LessonId);
                        return lessonProgress == null || lessonProgress.PercentComplete < 100;
                    });

                    if (currentLesson != null)
                    {
                        // Navigate to lesson
                        await Shell.Current.GoToAsync($"lesson?lessonId={currentLesson.LessonId}&courseId={CurrentCourse.CourseId}");
                    }
                    else
                    {
                        // All lessons complete, go to course detail
                        await Shell.Current.GoToAsync($"courseDetail?courseId={CurrentCourse.CourseId}");
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to continue learning: {ex.Message}", "OK");
            }
        }

        private async Task OnCourseSelected(CourseModel course)
        {
            if (course == null)
                return;

            // Navigate to course detail
            await Shell.Current.GoToAsync($"courseDetail?courseId={course.CourseId}");
        }

        private async Task ViewLeaderboardAsync()
        {
            // Navigate to leaderboard tab
            await Shell.Current.GoToAsync("//leaderboard");
        }
    }
}
