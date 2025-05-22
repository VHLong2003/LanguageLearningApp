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
                    // When courseId is set via QueryProperty, load data
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

                // Load current user data
                _currentUser = await _userService.GetUserByIdAsync(userId, idToken);

                // Load course details
                Course = await _courseService.GetCourseByIdAsync(CourseId, idToken);

                if (Course != null)
                {
                    // Load lessons and progress
                    await LoadLessonsAndProgressAsync();

                    // Update course progress
                    CourseProgress = await _lessonProgressService.GetOverallCourseProgressAsync(userId, CourseId, idToken) / 100.0;
                }
                else
                {
                    ErrorMessage = "Course not found";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading course: {ex.Message}";
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
                ErrorMessage = $"Error refreshing lessons: {ex.Message}";
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

            // Load all lessons for this course
            var lessons = await _lessonService.GetLessonsByCourseIdAsync(CourseId, idToken);

            // Sort by order
            lessons.Sort((a, b) => a.Order.CompareTo(b.Order));

            // Get progress info for all lessons in this course
            var lessonProgressList = await _lessonProgressService.GetUserLessonProgressAsync(userId, CourseId, idToken);

            // Also get old progress records for backward compatibility and stats
            var progressList = await _progressService.GetUserProgressAsync(userId, idToken);
            var courseProgress = progressList.Where(p => p.CourseId == CourseId).ToList();

            // Track course statistics
            CompletedLessons = 0;
            TotalPoints = 0;
            TotalTime = 0;

            LessonsWithProgress.Clear();
            int unlocked = 0;
            bool foundNextLesson = false;
            int previousLessonComplete = 1; // First lesson is always unlocked

            foreach (var lesson in lessons)
            {
                var lessonProgress = lessonProgressList.FirstOrDefault(p => p.LessonId == lesson.LessonId);
                var oldProgress = courseProgress.FirstOrDefault(p => p.LessonId == lesson.LessonId);

                bool isCompleted = lessonProgress?.IsCompleted ?? false;
                double percentComplete = lessonProgress?.PercentComplete ?? 0;
                int earnedPoints = lessonProgress?.EarnedPoints ?? 0;

                // Fallback to old progress model if needed
                if (!isCompleted && oldProgress != null && oldProgress.PercentComplete == 100)
                {
                    isCompleted = true;
                    percentComplete = 100;
                    earnedPoints = oldProgress.EarnedPoints;
                }

                bool isUnlocked = lesson.Order == 1 || // First lesson is always unlocked
                                (_currentUser.Points >= lesson.RequiredPointsToUnlock && previousLessonComplete == 1);

                // Update course statistics
                if (isCompleted)
                {
                    CompletedLessons++;
                    TotalPoints += earnedPoints;
                    TotalTime += lessonProgress?.TimeSpent ?? 0;
                }

                if (isUnlocked)
                    unlocked++;

                previousLessonComplete = isCompleted ? 1 : 0;

                // Find next lesson to complete
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

            // If all lessons are completed
            if (!foundNextLesson)
            {
                NextLessonTitle = "All lessons completed!";
            }
        }

        private async Task LessonSelectedAsync(LessonProgressItem lessonProgress)
        {
            if (lessonProgress == null)
                return;

            if (!lessonProgress.IsUnlocked)
            {
                // Show message about locked lesson
                await App.Current.MainPage.DisplayAlert(
                    "Lesson Locked",
                    $"You need {lessonProgress.Lesson.RequiredPointsToUnlock} points to unlock this lesson. You currently have {_currentUser.Points} points.",
                    "OK");
                return;
            }

            // Navigate to lesson
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
                    Title = $"Check out this course: {Course.Title}",
                    Text = $"I'm learning {Course.Title} on Language Learning App. It's a {Course.Type} course with {Course.TotalLessons} lessons. Join me!"
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Sharing Failed", $"Could not share the course: {ex.Message}", "OK");
            }
        }
    }

    // Helper class to represent lesson progress in the UI
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
                    return "Completed";
                if (!IsUnlocked)
                    return "Locked";
                if (CurrentQuestionIndex > 0)
                    return "In Progress";
                return "Not Started";
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
