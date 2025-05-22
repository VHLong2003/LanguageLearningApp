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
    public class LessonViewModel : BaseViewModel
    {
        private readonly LessonService _lessonService;
        private readonly ProgressService _progressService;
        private readonly UserService _userService;
        private readonly CourseService _courseService;
        private readonly LessonProgressService _lessonProgressService;
        private readonly BadgeService _badgeService;

        private string _lessonId;
        private string _courseId;
        private LessonModel _currentLesson;
        private CourseModel _currentCourse;
        private ObservableCollection<QuestionModel> _questions;
        private QuestionModel _currentQuestion;
        private int _currentQuestionIndex;
        private int _totalQuestions;
        private bool _isLoading;
        private bool _isCompleted;
        private string _selectedAnswer;
        private bool _isAnswerCorrect;
        private bool _hasAnswered;
        private int _correctAnswers;
        private int _earnedPoints;
        private double _progressPercent;
        private string _explanation;
        private LessonProgressModel _currentProgress;

        public LessonModel CurrentLesson
        {
            get => _currentLesson;
            set => SetProperty(ref _currentLesson, value);
        }

        public CourseModel CurrentCourse
        {
            get => _currentCourse;
            set => SetProperty(ref _currentCourse, value);
        }

        public ObservableCollection<QuestionModel> Questions
        {
            get => _questions;
            set => SetProperty(ref _questions, value);
        }

        public QuestionModel CurrentQuestion
        {
            get => _currentQuestion;
            set => SetProperty(ref _currentQuestion, value);
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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        public string SelectedAnswer
        {
            get => _selectedAnswer;
            set => SetProperty(ref _selectedAnswer, value);
        }

        public bool IsAnswerCorrect
        {
            get => _isAnswerCorrect;
            set => SetProperty(ref _isAnswerCorrect, value);
        }

        public bool HasAnswered
        {
            get => _hasAnswered;
            set => SetProperty(ref _hasAnswered, value);
        }

        public int CorrectAnswers
        {
            get => _correctAnswers;
            set => SetProperty(ref _correctAnswers, value);
        }

        public int EarnedPoints
        {
            get => _earnedPoints;
            set => SetProperty(ref _earnedPoints, value);
        }

        public double ProgressPercent
        {
            get => _progressPercent;
            set => SetProperty(ref _progressPercent, value);
        }

        public string Explanation
        {
            get => _explanation;
            set => SetProperty(ref _explanation, value);
        }

        // Progress tracking
        private DateTime _startTime;
        private int _timeSpent;

        // Commands
        public ICommand AnswerCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand FinishLessonCommand { get; }
        public ICommand RetryLessonCommand { get; }
        public ICommand ContinueToNextLessonCommand { get; }

        public LessonViewModel(
            LessonService lessonService,
            ProgressService progressService,
            UserService userService,
            CourseService courseService,
            LessonProgressService lessonProgressService,
            BadgeService badgeService)
        {
            _lessonService = lessonService;
            _progressService = progressService;
            _userService = userService;
            _courseService = courseService;
            _lessonProgressService = lessonProgressService;
            _badgeService = badgeService;

            Questions = new ObservableCollection<QuestionModel>();
            _startTime = DateTime.Now;

            AnswerCommand = new Command<string>(OnAnswerSelected);
            NextQuestionCommand = new Command(async () => await OnNextQuestionAsync());
            FinishLessonCommand = new Command(async () => await FinishLessonAsync());
            RetryLessonCommand = new Command(async () => await LoadLessonAsync(_lessonId));
            ContinueToNextLessonCommand = new Command(async () => await NavigateToNextLessonAsync());
        }

        public async Task InitializeAsync(string lessonId, string courseId)
        {
            _lessonId = lessonId;
            _courseId = courseId;

            await LoadLessonAsync(lessonId);
        }

        private async Task LoadLessonAsync(string lessonId)
        {
            IsLoading = true;

            // Reset lesson state
            CurrentQuestionIndex = 0;
            CorrectAnswers = 0;
            EarnedPoints = 0;
            ProgressPercent = 0;
            HasAnswered = false;
            IsAnswerCorrect = false;
            IsCompleted = false;
            SelectedAnswer = null;
            Explanation = null;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Load lesson details
                var lesson = await _lessonService.GetLessonByIdAsync(lessonId, idToken);
                if (lesson == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to load lesson", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                CurrentLesson = lesson;

                // Load course details
                var course = await _courseService.GetCourseByIdAsync(_courseId, idToken);
                CurrentCourse = course;

                // Load questions for this lesson
                var questions = await _lessonService.GetLessonQuestions(lessonId, idToken);

                Questions.Clear();
                foreach (var question in questions.OrderBy(q => q.Order))
                {
                    Questions.Add(question);
                }

                TotalQuestions = Questions.Count;

                // Load existing progress or create new
                _currentProgress = await _lessonProgressService.GetLessonProgressAsync(userId, lessonId, idToken);

                if (_currentProgress != null)
                {
                    // If lesson was started but not completed, resume progress
                    if (!_currentProgress.IsCompleted)
                    {
                        CurrentQuestionIndex = _currentProgress.CurrentQuestionIndex;
                        CorrectAnswers = _currentProgress.CorrectAnswers;
                        EarnedPoints = _currentProgress.EarnedPoints;
                        ProgressPercent = _currentProgress.PercentComplete / 100.0;
                    }

                    // Update access time
                    _currentProgress.LastAccessDate = DateTime.Now;
                    await _lessonProgressService.CreateOrUpdateLessonProgressAsync(_currentProgress, idToken);
                }
                else
                {
                    // Create new progress entry
                    _currentProgress = new LessonProgressModel
                    {
                        UserId = userId,
                        LessonId = lessonId,
                        CourseId = _courseId,
                        StartedDate = DateTime.Now,
                        LastAccessDate = DateTime.Now,
                        IsCompleted = false,
                        PercentComplete = 0,
                        CurrentQuestionIndex = 0,
                        TotalQuestions = TotalQuestions,
                        CorrectAnswers = 0,
                        EarnedPoints = 0,
                        TimeSpent = 0,
                        Attempts = 1
                    };

                    await _lessonProgressService.CreateOrUpdateLessonProgressAsync(_currentProgress, idToken);
                }

                // Set current question based on progress
                if (TotalQuestions > 0)
                {
                    if (CurrentQuestionIndex < TotalQuestions)
                    {
                        CurrentQuestion = Questions[CurrentQuestionIndex];
                    }
                    else if (_currentProgress.IsCompleted)
                    {
                        // If lesson was already completed, go to completion screen
                        IsCompleted = true;
                        ProgressPercent = 1.0;
                        CorrectAnswers = _currentProgress.CorrectAnswers;
                        EarnedPoints = _currentProgress.EarnedPoints;
                    }
                }

                // Record start time for this session
                _startTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnAnswerSelected(string answer)
        {
            if (HasAnswered || CurrentQuestion == null)
                return;

            HasAnswered = true;
            SelectedAnswer = answer;

            // Check if the answer is correct
            IsAnswerCorrect = answer == CurrentQuestion.CorrectAnswer;

            // Award points for correct answer
            if (IsAnswerCorrect)
            {
                CorrectAnswers++;
                EarnedPoints += CurrentQuestion.Points;

                // Show explanation if available
                if (!string.IsNullOrEmpty(CurrentQuestion.Explanation))
                {
                    Explanation = CurrentQuestion.Explanation;
                }
            }
            else
            {
                // Show explanation for wrong answers
                Explanation = $"The correct answer is: {CurrentQuestion.CorrectAnswer}\n\n{CurrentQuestion.Explanation}";
            }

            // Update progress for this question
            UpdateProgressAfterAnswer();
        }

        private async void UpdateProgressAfterAnswer()
        {
            if (_currentProgress != null)
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                _currentProgress.LastAccessDate = DateTime.Now;
                _currentProgress.CorrectAnswers = CorrectAnswers;
                _currentProgress.EarnedPoints = EarnedPoints;

                // Calculate time spent so far
                var currentSession = (DateTime.Now - _startTime).TotalSeconds;
                _currentProgress.TimeSpent += (int)currentSession;

                await _lessonProgressService.CreateOrUpdateLessonProgressAsync(_currentProgress, idToken);
            }
        }

        private async Task OnNextQuestionAsync()
        {
            // Move to the next question
            HasAnswered = false;
            IsAnswerCorrect = false;
            SelectedAnswer = null;
            Explanation = null;

            CurrentQuestionIndex++;

            if (CurrentQuestionIndex < TotalQuestions)
            {
                CurrentQuestion = Questions[CurrentQuestionIndex];
                ProgressPercent = (double)CurrentQuestionIndex / TotalQuestions;

                // Update progress in database
                await UpdateProgressForNextQuestion();
            }
            else
            {
                // All questions completed
                IsCompleted = true;
                ProgressPercent = 1.0;

                // Calculate time spent
                _timeSpent = (int)(DateTime.Now - _startTime).TotalSeconds;
                if (_currentProgress != null)
                {
                    _currentProgress.TimeSpent += _timeSpent;
                }
            }
        }

        private async Task UpdateProgressForNextQuestion()
        {
            if (_currentProgress != null)
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                _currentProgress.CurrentQuestionIndex = CurrentQuestionIndex;
                _currentProgress.PercentComplete = ProgressPercent * 100;
                _currentProgress.LastAccessDate = DateTime.Now;

                await _lessonProgressService.CreateOrUpdateLessonProgressAsync(_currentProgress, idToken);
            }
        }

        private async Task FinishLessonAsync()
        {
            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Calculate total time spent including previous sessions
                int totalTimeSpent = _currentProgress?.TimeSpent ?? 0;
                totalTimeSpent += (int)(DateTime.Now - _startTime).TotalSeconds;

                // Mark lesson as completed in LessonProgress
                bool lessonProgressUpdated = await _lessonProgressService.CompleteLessonProgressAsync(
                    userId,
                    _lessonId,
                    _courseId,
                    CorrectAnswers,
                    TotalQuestions,
                    EarnedPoints,
                    totalTimeSpent,
                    idToken);

                if (lessonProgressUpdated)
                {
                    // Also update the main Progress record for this user/course/lesson
                    var progress = new ProgressModel
                    {
                        UserId = userId,
                        CourseId = _courseId,
                        LessonId = _lessonId,
                        CompletedDate = DateTime.Now,
                        EarnedPoints = EarnedPoints,
                        PercentComplete = 100,
                        CorrectAnswers = CorrectAnswers,
                        TotalQuestions = TotalQuestions,
                        TimeSpent = totalTimeSpent,
                        LastAttemptDate = DateTime.Now
                    };

                    await _progressService.SaveProgressAsync(progress, idToken);

                    // Update user stats (points, streak, etc.)
                    var user = await _userService.GetUserByIdAsync(userId, idToken);

                    if (user != null)
                    {
                        // Add earned points
                        user.Points += EarnedPoints;

                        // Add coins reward
                        user.Coins += AppSettings.CoinsPerLessonCompleted;

                        // Update streak if applicable
                        DateTime lastActive = user.LastActive;
                        DateTime today = DateTime.Today;

                        if (lastActive.Date == today.AddDays(-1))
                        {
                            // User was active yesterday, increment streak
                            user.CurrentStreak++;
                        }
                        else if (lastActive.Date < today.AddDays(-1))
                        {
                            // User missed a day or more, reset streak
                            user.CurrentStreak = 1;
                        }
                        // If lastActive is today, streak remains unchanged

                        // Update last active date
                        user.LastActive = DateTime.Now;

                        // Save user updates
                        await _userService.UpdateUserAsync(user, idToken);

                        // Check for badge awards
                        await _badgeService.CheckAndAwardBadges(user, idToken);

                        // Check for course completion badge
                        double courseProgress = await _lessonProgressService.GetOverallCourseProgressAsync(userId, _courseId, idToken);
                        if (courseProgress >= 100)
                        {
                            await _badgeService.AwardCourseCompletionBadge(userId, _courseId, idToken);
                        }
                    }

                    // Navigate back to course detail with completed flag
                    await Shell.Current.GoToAsync($"..?completed=true");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to save progress", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to save progress: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NavigateToNextLessonAsync()
        {
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Get all lessons for current course
                var lessons = await _lessonService.GetLessonsByCourseIdAsync(_courseId, idToken);

                // Sort by order
                lessons = lessons.OrderBy(l => l.Order).ToList();

                // Find current lesson index
                int currentIndex = lessons.FindIndex(l => l.LessonId == _lessonId);

                if (currentIndex >= 0 && currentIndex < lessons.Count - 1)
                {
                    // There is a next lesson
                    var nextLesson = lessons[currentIndex + 1];

                    // Navigate to next lesson
                    await Shell.Current.GoToAsync($"../lesson?lessonId={nextLesson.LessonId}&courseId={_courseId}");
                }
                else
                {
                    // This was the last lesson
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
            }
        }
    }
}
