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
    [QueryProperty(nameof(LessonId), "lessonId")]
    [QueryProperty(nameof(CourseId), "courseId")]
    public class LessonViewModel : BaseViewModel
    {
        #region Fields
        private readonly LessonService _lessonService;
        private readonly QuestionService _questionService;
        private readonly LessonProgressService _lessonProgressService;
        private readonly CourseService _courseService;
        private readonly UserService _userService;

        private string _lessonId;
        private string _courseId;
        private LessonModel _lesson;
        private CourseModel _course;
        private ObservableCollection<QuestionModel> _questions;
        private QuestionModel _currentQuestion;
        private string _selectedAnswer;
        private string _userAnswer;
        private string _explanation;
        private bool _isAnswerCorrect;
        private bool _hasAnswered;
        private bool _isCompleted;
        private double _progressPercent;
        private int _currentQuestionIndex;
        private int _totalQuestions;
        private int _correctAnswers;
        private int _earnedPoints;
        private bool _isLoading;
        private bool _isRefreshing;
        private string _errorMessage;
        #endregion

        #region Properties
        public string LessonId
        {
            get => _lessonId;
            set
            {
                if (SetProperty(ref _lessonId, value) && !string.IsNullOrEmpty(value))
                {
                    Task.Run(async () => await LoadLessonAsync());
                }
            }
        }

        public string CourseId
        {
            get => _courseId;
            set => SetProperty(ref _courseId, value);
        }

        public LessonModel Lesson
        {
            get => _lesson;
            set => SetProperty(ref _lesson, value);
        }

        public CourseModel Course
        {
            get => _course;
            set => SetProperty(ref _course, value);
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

        public string SelectedAnswer
        {
            get => _selectedAnswer;
            set
            {
                SetProperty(ref _selectedAnswer, value);
                HasAnswered = !string.IsNullOrEmpty(value) || !string.IsNullOrEmpty(UserAnswer);
            }
        }

        public string UserAnswer
        {
            get => _userAnswer;
            set
            {
                SetProperty(ref _userAnswer, value);
                HasAnswered = !string.IsNullOrEmpty(value) || !string.IsNullOrEmpty(SelectedAnswer);
            }
        }

        public string Explanation
        {
            get => _explanation;
            set => SetProperty(ref _explanation, value);
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

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        public double ProgressPercent
        {
            get => _progressPercent;
            set => SetProperty(ref _progressPercent, value);
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

        public string CorrectAndTotalText => $"{CorrectAnswers}/{TotalQuestions}";
        #endregion

        #region Commands
        public ICommand AnswerCommand { get; }
        public ICommand SubmitAnswerCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand FinishLessonCommand { get; }
        public ICommand RetryLessonCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion

        #region Constructor
        public LessonViewModel(
            LessonService lessonService,
            QuestionService questionService,
            LessonProgressService lessonProgressService,
            CourseService courseService,
            UserService userService)
        {
            _lessonService = lessonService;
            _questionService = questionService;
            _lessonProgressService = lessonProgressService;
            _courseService = courseService;
            _userService = userService;

            Questions = new ObservableCollection<QuestionModel>();

            AnswerCommand = new Command<string>(async (answer) => await AnswerAsync(answer));
            SubmitAnswerCommand = new Command(async () => await SubmitAnswerAsync());
            NextQuestionCommand = new Command(async () => await NextQuestionAsync());
            FinishLessonCommand = new Command(async () => await FinishLessonAsync());
            RetryLessonCommand = new Command(async () => await RetryLessonAsync());
            RefreshCommand = new Command(async () => await LoadLessonAsync());
        }
        #endregion

        #region Public Methods
        public async Task InitializeAsync()
        {
            await LoadLessonAsync();
        }
        #endregion

        #region Private Methods
        private async Task LoadLessonAsync()
        {
            IsLoading = true;
            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(userId))
                {
                    ErrorMessage = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                    return;
                }

                // Tải thông tin khóa học
                Course = await _courseService.GetCourseByIdAsync(CourseId, idToken);
                if (Course == null)
                {
                    ErrorMessage = "Không tìm thấy khóa học.";
                    return;
                }

                // Tải thông tin bài học
                Lesson = await _lessonService.GetLessonByIdAsync(LessonId, idToken);
                if (Lesson == null)
                {
                    ErrorMessage = "Không tìm thấy bài học.";
                    return;
                }

                // Tải danh sách câu hỏi
                var questions = await _questionService.GetQuestionsByLessonIdAsync(LessonId, idToken);
                Questions.Clear();
                foreach (var question in questions.OrderBy(q => q.Order))
                {
                    Questions.Add(question);
                }

                TotalQuestions = Questions.Count;

                // Tải tiến trình bài học
                var progress = await _lessonProgressService.GetLessonProgressAsync(userId, LessonId, idToken);
                CurrentQuestionIndex = progress?.CurrentQuestionIndex ?? 0;
                CorrectAnswers = progress?.CorrectAnswers ?? 0;
                EarnedPoints = progress?.EarnedPoints ?? 0;
                IsCompleted = progress?.IsCompleted ?? false;
                ProgressPercent = TotalQuestions > 0 ? (double)CurrentQuestionIndex / TotalQuestions : 0;

                // Cập nhật trạng thái giao diện
                if (IsCompleted)
                {
                    CurrentQuestion = null;
                    HasAnswered = false;
                    SelectedAnswer = null;
                    UserAnswer = null;
                    Explanation = null;
                }
                else if (CurrentQuestionIndex == 0)
                {
                    CurrentQuestion = null; // Hiển thị phần mở đầu
                }
                else if (CurrentQuestionIndex <= TotalQuestions)
                {
                    CurrentQuestion = Questions[CurrentQuestionIndex - 1];
                    HasAnswered = false;
                    SelectedAnswer = null;
                    UserAnswer = null;
                    Explanation = null;
                }
                else
                {
                    IsCompleted = true;
                    CurrentQuestion = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải bài học: {ex.Message}";
                Console.WriteLine($"LoadLessonAsync Error: {ex}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        private async Task AnswerAsync(string answer)
        {
            if (HasAnswered || CurrentQuestion == null)
                return;

            SelectedAnswer = answer;
            HasAnswered = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Nếu câu hỏi là TrueFalse thì chuyển về bool so sánh
                if (CurrentQuestion.Type == QuestionType.TrueFalse)
                {
                    // Đáp án trong DB là "True"/"False" (string) hoặc bool? => chuẩn hóa thành chuỗi chữ thường hoặc bool đều được
                    var correct = CurrentQuestion.CorrectAnswer?.Trim().ToLower();
                    var userAns = answer?.Trim().ToLower();
                    IsAnswerCorrect = (userAns == correct);

                    // Nếu đáp án gốc lưu là bool: (không thường dùng trong Firebase, nhưng nếu có)
                    // bool correctBool = bool.TryParse(CurrentQuestion.CorrectAnswer, out var tmp) && tmp;
                    // bool userAnsBool = bool.TryParse(answer, out var tmp2) && tmp2;
                    // IsAnswerCorrect = (correctBool == userAnsBool);
                }
                else
                {
                    // Các dạng khác
                    IsAnswerCorrect = string.Equals(answer?.Trim(), CurrentQuestion.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
                }

                Explanation = CurrentQuestion.Explanation;

                if (IsAnswerCorrect)
                {
                    CorrectAnswers++;
                    EarnedPoints += 10; // hoặc CurrentQuestion.Points
                }

                // Cập nhật tiến trình
                var progress = new LessonProgressModel
                {
                    UserId = userId,
                    LessonId = LessonId,
                    CourseId = CourseId,
                    CurrentQuestionIndex = CurrentQuestionIndex,
                    CorrectAnswers = CorrectAnswers,
                    EarnedPoints = EarnedPoints,
                    PercentComplete = TotalQuestions > 0 ? (CurrentQuestionIndex * 100.0 / TotalQuestions) : 0,
                    IsCompleted = CurrentQuestionIndex >= TotalQuestions,
                    TimeSpent = 30, // Giả sử 30 giây mỗi câu
                    TotalQuestions = TotalQuestions
                };

                await _lessonProgressService.UpdateLessonProgressAsync(progress, idToken);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi xử lý câu trả lời: {ex.Message}";
                HasAnswered = false;
                SelectedAnswer = null;
                Console.WriteLine($"AnswerAsync Error: {ex}");
            }
        }


        private async Task SubmitAnswerAsync()
        {
            if (HasAnswered || CurrentQuestion == null || string.IsNullOrEmpty(UserAnswer))
                return;

            HasAnswered = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                IsAnswerCorrect = string.Equals(UserAnswer.Trim(), CurrentQuestion.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                Explanation = CurrentQuestion.Explanation;

                if (IsAnswerCorrect)
                {
                    CorrectAnswers++;
                    EarnedPoints += 10;
                }

                // Cập nhật tiến trình
                var progress = new LessonProgressModel
                {
                    UserId = userId,
                    LessonId = LessonId,
                    CourseId = CourseId,
                    CurrentQuestionIndex = CurrentQuestionIndex,
                    CorrectAnswers = CorrectAnswers,
                    EarnedPoints = EarnedPoints,
                    PercentComplete = TotalQuestions > 0 ? (CurrentQuestionIndex * 100.0 / TotalQuestions) : 0,
                    IsCompleted = CurrentQuestionIndex >= TotalQuestions,
                    TimeSpent = 30,
                    TotalQuestions = TotalQuestions
                };

                await _lessonProgressService.UpdateLessonProgressAsync(progress, idToken);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi gửi câu trả lời: {ex.Message}";
                HasAnswered = false;
                UserAnswer = null;
                Console.WriteLine($"SubmitAnswerAsync Error: {ex}");
            }
        }

        private async Task NextQuestionAsync()
        {
            if (CurrentQuestionIndex >= TotalQuestions)
            {
                IsCompleted = true;
                CurrentQuestion = null;
                return;
            }

            try
            {
                CurrentQuestionIndex++;
                ProgressPercent = TotalQuestions > 0 ? (double)CurrentQuestionIndex / TotalQuestions : 0;

                if (CurrentQuestionIndex <= TotalQuestions)
                {
                    CurrentQuestion = Questions[CurrentQuestionIndex - 1];
                    HasAnswered = false;
                    SelectedAnswer = null;
                    UserAnswer = null;
                    Explanation = null;
                    IsAnswerCorrect = false;

                    // Cập nhật tiến trình
                    var idToken = LocalStorageHelper.GetItem("idToken");
                    var userId = LocalStorageHelper.GetItem("userId");

                    var progress = new LessonProgressModel
                    {
                        UserId = userId,
                        LessonId = LessonId,
                        CourseId = CourseId,
                        CurrentQuestionIndex = CurrentQuestionIndex,
                        CorrectAnswers = CorrectAnswers,
                        EarnedPoints = EarnedPoints,
                        PercentComplete = ProgressPercent * 100,
                        IsCompleted = CurrentQuestionIndex >= TotalQuestions,
                        TimeSpent = 30,
                        TotalQuestions = TotalQuestions
                    };

                    await _lessonProgressService.UpdateLessonProgressAsync(progress, idToken);
                }
                else
                {
                    IsCompleted = true;
                    CurrentQuestion = null;

                    // Cập nhật trạng thái hoàn thành
                    var idToken = LocalStorageHelper.GetItem("idToken");
                    var userId = LocalStorageHelper.GetItem("userId");

                    var progress = new LessonProgressModel
                    {
                        UserId = userId,
                        LessonId = LessonId,
                        CourseId = CourseId,
                        CurrentQuestionIndex = CurrentQuestionIndex,
                        CorrectAnswers = CorrectAnswers,
                        EarnedPoints = EarnedPoints,
                        PercentComplete = 100,
                        IsCompleted = true,
                        TimeSpent = 30,
                        TotalQuestions = TotalQuestions
                    };

                    await _lessonProgressService.UpdateLessonProgressAsync(progress, idToken);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi chuyển câu hỏi: {ex.Message}";
                Console.WriteLine($"NextQuestionAsync Error: {ex}");
            }
        }

        private async Task FinishLessonAsync()
        {
            try
            {
                await Shell.Current.GoToAsync($"courseDetail?courseId={CourseId}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi kết thúc bài học: {ex.Message}";
                Console.WriteLine($"FinishLessonAsync Error: {ex}");
            }
        }

        private async Task RetryLessonAsync()
        {
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Đặt lại tiến trình bài học
                var progress = new LessonProgressModel
                {
                    UserId = userId,
                    LessonId = LessonId,
                    CourseId = CourseId,
                    CurrentQuestionIndex = 0,
                    CorrectAnswers = 0,
                    EarnedPoints = 0,
                    PercentComplete = 0,
                    IsCompleted = false,
                    TimeSpent = 0,
                    TotalQuestions = TotalQuestions
                };

                await _lessonProgressService.UpdateLessonProgressAsync(progress, idToken);

                // Tải lại bài học
                await LoadLessonAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi làm lại bài học: {ex.Message}";
                Console.WriteLine($"RetryLessonAsync Error: {ex}");
            }
        }
        #endregion
    }
}