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
    [QueryProperty(nameof(LessonId), "lessonId")]
    [QueryProperty(nameof(CourseId), "courseId")]
    public class AdminQuestionViewModel : BaseViewModel
    {
        private readonly QuestionService _questionService;
        private readonly LessonService _lessonService;
        private readonly StorageService _storageService;

        private string _lessonId;
        private string _courseId;
        private LessonModel _currentLesson;
        private ObservableCollection<QuestionModel> _questions;
        private QuestionModel _selectedQuestion;
        private bool _isRefreshing;
        private bool _isLoading;
        private bool _isEditing;
        private bool _isNewQuestion;
        private string _errorMessage;
        private ObservableCollection<QuestionType> _questionTypes;
        private ObservableCollection<string> _options;
        private string _newOption;

        // Form fields for editing/creating question
        private string _questionId;
        private string _content;
        private string _imageUrl;
        private string _audioUrl;
        private QuestionType _type;
        private string _correctAnswer;
        private string _explanation;
        private int _points;
        private int _order;
        private int _timeLimit;

        public string LessonId
        {
            get => _lessonId;
            set
            {
                if (SetProperty(ref _lessonId, value) && !string.IsNullOrEmpty(value))
                {
                    Task.Run(async () => await LoadLessonAndQuestionsAsync());
                }
            }
        }

        public string CourseId
        {
            get => _courseId;
            set => SetProperty(ref _courseId, value);
        }

        public LessonModel CurrentLesson
        {
            get => _currentLesson;
            set => SetProperty(ref _currentLesson, value);
        }

        public ObservableCollection<QuestionModel> Questions
        {
            get => _questions;
            set => SetProperty(ref _questions, value);
        }

        public QuestionModel SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                if (SetProperty(ref _selectedQuestion, value) && value != null)
                {
                    QuestionId = value.QuestionId;
                    Content = value.Content;
                    ImageUrl = value.ImageUrl;
                    AudioUrl = value.AudioUrl;
                    Type = value.Type;
                    CorrectAnswer = value.CorrectAnswer;
                    Explanation = value.Explanation;
                    Points = value.Points;
                    Order = value.Order;
                    TimeLimit = value.TimeLimit;

                    Options.Clear();
                    if (value.Options != null)
                    {
                        foreach (var option in value.Options)
                        {
                            Options.Add(option);
                        }
                    }

                    IsEditing = true;
                    IsNewQuestion = false;
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

        public bool IsNewQuestion
        {
            get => _isNewQuestion;
            set => SetProperty(ref _isNewQuestion, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<QuestionType> QuestionTypes
        {
            get => _questionTypes;
            set => SetProperty(ref _questionTypes, value);
        }

        public ObservableCollection<string> Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }

        public string NewOption
        {
            get => _newOption;
            set => SetProperty(ref _newOption, value);
        }

        public string QuestionId
        {
            get => _questionId;
            set => SetProperty(ref _questionId, value);
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

        public string AudioUrl
        {
            get => _audioUrl;
            set => SetProperty(ref _audioUrl, value);
        }

        public QuestionType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string CorrectAnswer
        {
            get => _correctAnswer;
            set => SetProperty(ref _correctAnswer, value);
        }

        public string Explanation
        {
            get => _explanation;
            set => SetProperty(ref _explanation, value);
        }

        public int Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        public int Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public int TimeLimit
        {
            get => _timeLimit;
            set => SetProperty(ref _timeLimit, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand CreateQuestionCommand { get; }
        public ICommand SaveQuestionCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteQuestionCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand AddOptionCommand { get; }
        public ICommand RemoveOptionCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand BackToLessonsCommand { get; }

        public AdminQuestionViewModel(
            QuestionService questionService,
            LessonService lessonService,
            StorageService storageService)
        {
            _questionService = questionService;
            _lessonService = lessonService;
            _storageService = storageService;

            Questions = new ObservableCollection<QuestionModel>();
            Options = new ObservableCollection<string>();
            QuestionTypes = new ObservableCollection<QuestionType>
            {
                QuestionType.MultipleChoice,
                QuestionType.TrueFalse,
                QuestionType.FillInTheBlank,
                QuestionType.Matching,
                QuestionType.ShortAnswer,
                QuestionType.VoiceRecording,
                QuestionType.Arrangement
            };

            RefreshCommand = new Command(async () => await LoadQuestionsAsync());
            CreateQuestionCommand = new Command(CreateNewQuestion);
            SaveQuestionCommand = new Command(async () => await SaveQuestionAsync());
            CancelEditCommand = new Command(CancelEdit);
            DeleteQuestionCommand = new Command<QuestionModel>(async (question) => await DeleteQuestionAsync(question));
            PickImageCommand = new Command(async () => await PickImageAsync());
            AddOptionCommand = new Command(AddOption);
            RemoveOptionCommand = new Command<string>(RemoveOption);
            MoveUpCommand = new Command<QuestionModel>(async (question) => await MoveUpAsync(question));
            MoveDownCommand = new Command<QuestionModel>(async (question) => await MoveDownAsync(question));
            BackToLessonsCommand = new Command(async () => await Shell.Current.GoToAsync($"lessonManagement?courseId={CourseId}"));
        }

        public async Task LoadLessonAndQuestionsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                CurrentLesson = await _lessonService.GetLessonByIdAsync(LessonId, idToken);
                if (CurrentLesson == null)
                {
                    ErrorMessage = "Không tìm thấy bài học";
                    return;
                }

                await LoadQuestionsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải bài học: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadQuestionsAsync()
        {
            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                var lessonQuestions = await _questionService.GetQuestionsByLessonIdAsync(LessonId, idToken);
                if (lessonQuestions == null) lessonQuestions = new List<QuestionModel>();

                lessonQuestions.Sort((a, b) => a.Order.CompareTo(b.Order));
                Questions.Clear();
                foreach (var question in lessonQuestions)
                {
                    Questions.Add(question);
                    Console.WriteLine($"Đã thêm câu hỏi vào UI: {question.Content} (Thứ tự: {question.Order})");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải câu hỏi: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void CreateNewQuestion()
        {
            QuestionId = string.Empty;
            Content = string.Empty;
            ImageUrl = string.Empty;
            AudioUrl = string.Empty;
            Type = QuestionType.MultipleChoice;
            CorrectAnswer = string.Empty;
            Explanation = string.Empty;
            Points = 10;
            Order = Questions.Count + 1;
            TimeLimit = 30;

            Options.Clear();
            Options.Add("Lựa chọn 1");
            Options.Add("Lựa chọn 2");
            Options.Add("Lựa chọn 3");
            Options.Add("Lựa chọn 4");

            IsNewQuestion = true;
            IsEditing = true;
            SelectedQuestion = null;
            ErrorMessage = string.Empty;
        }

        private async Task SaveQuestionAsync()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                ErrorMessage = "Nội dung câu hỏi là bắt buộc";
                return;
            }

            if (Type == QuestionType.MultipleChoice && Options.Count < 2)
            {
                ErrorMessage = "Câu hỏi trắc nghiệm cần ít nhất 2 lựa chọn";
                return;
            }

            if (string.IsNullOrWhiteSpace(CorrectAnswer))
            {
                ErrorMessage = "Đáp án đúng là bắt buộc";
                return;
            }

            if (Points <= 0)
            {
                ErrorMessage = "Điểm số phải lớn hơn 0";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("Người dùng chưa được xác thực. Vui lòng đăng nhập lại.");

                if (IsNewQuestion)
                {
                    var newQuestion = new QuestionModel
                    {
                        LessonId = LessonId,
                        Content = Content,
                        ImageUrl = ImageUrl,
                        AudioUrl = AudioUrl,
                        Type = Type,
                        Options = new List<string>(Options),
                        CorrectAnswer = CorrectAnswer,
                        Explanation = Explanation,
                        Points = Points,
                        Order = Order,
                        TimeLimit = TimeLimit
                    };

                    var questionId = await _questionService.CreateQuestionAsync(newQuestion, idToken);
                    if (!string.IsNullOrEmpty(questionId))
                    {
                        newQuestion.QuestionId = questionId;
                        Questions.Add(newQuestion);
                        Console.WriteLine($"Đã tạo câu hỏi mới với ID: {questionId}");
                        CancelEdit();
                    }
                    else
                    {
                        ErrorMessage = "Không thể tạo câu hỏi";
                        await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
                    }
                }
                else
                {
                    if (_selectedQuestion != null)
                    {
                        _selectedQuestion.Content = Content;
                        _selectedQuestion.ImageUrl = ImageUrl;
                        _selectedQuestion.AudioUrl = AudioUrl;
                        _selectedQuestion.Type = Type;
                        _selectedQuestion.Options = new List<string>(Options);
                        _selectedQuestion.CorrectAnswer = CorrectAnswer;
                        _selectedQuestion.Explanation = Explanation;
                        _selectedQuestion.Points = Points;
                        _selectedQuestion.Order = Order;
                        _selectedQuestion.TimeLimit = TimeLimit;

                        var success = await _questionService.UpdateQuestionAsync(_selectedQuestion, idToken);
                        if (success)
                        {
                            Console.WriteLine($"Đã cập nhật câu hỏi: {_selectedQuestion.Content}");
                            CancelEdit();
                            await LoadQuestionsAsync();
                        }
                        else
                        {
                            ErrorMessage = "Không thể cập nhật câu hỏi";
                            await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi lưu câu hỏi: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            IsNewQuestion = false;
            SelectedQuestion = null;

            QuestionId = string.Empty;
            Content = string.Empty;
            ImageUrl = string.Empty;
            AudioUrl = string.Empty;
            Type = QuestionType.MultipleChoice;
            CorrectAnswer = string.Empty;
            Explanation = string.Empty;
            Points = 0;
            Order = 0;
            TimeLimit = 0;

            Options.Clear();
            NewOption = string.Empty;
            ErrorMessage = string.Empty;
        }

        private async Task DeleteQuestionAsync(QuestionModel question)
        {
            if (question == null) return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Xác nhận xóa",
                "Bạn có chắc chắn muốn xóa câu hỏi này?",
                "Có", "Không");

            if (!confirm) return;

            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var success = await _questionService.DeleteQuestionAsync(question.QuestionId, idToken);

                if (success)
                {
                    Questions.Remove(question);
                    await ReorderQuestionsAsync();
                    if (SelectedQuestion == question) CancelEdit();
                    Console.WriteLine($"Đã xóa câu hỏi với ID: {question.QuestionId}");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể xóa câu hỏi", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi xóa câu hỏi: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PickImageAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Chọn ảnh cho câu hỏi"
                });

                if (result != null)
                {
                    IsLoading = true;
                    var idToken = LocalStorageHelper.GetItem("idToken");
                    var imageUrl = await _storageService.UploadImageFromPickerAsync(result, "question_images", idToken);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        ImageUrl = imageUrl;
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể tải ảnh lên", "OK");
                    }
                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi chọn ảnh: {ex.Message}", "OK");
            }
        }

        private void AddOption()
        {
            if (string.IsNullOrWhiteSpace(NewOption)) return;
            Options.Add(NewOption);
            NewOption = string.Empty;
        }

        private void RemoveOption(string option)
        {
            if (string.IsNullOrWhiteSpace(option)) return;
            Options.Remove(option);
            if (CorrectAnswer == option) CorrectAnswer = string.Empty;
        }

        private async Task MoveUpAsync(QuestionModel question)
        {
            if (question == null) return;
            var index = Questions.IndexOf(question);
            if (index <= 0) return;

            var previousQuestion = Questions[index - 1];
            int tempOrder = question.Order;
            question.Order = previousQuestion.Order;
            previousQuestion.Order = tempOrder;

            var idToken = LocalStorageHelper.GetItem("idToken");
            await _questionService.UpdateQuestionAsync(question, idToken);
            await _questionService.UpdateQuestionAsync(previousQuestion, idToken);
            await LoadQuestionsAsync();
        }

        private async Task MoveDownAsync(QuestionModel question)
        {
            if (question == null) return;
            var index = Questions.IndexOf(question);
            if (index >= Questions.Count - 1) return;

            var nextQuestion = Questions[index + 1];
            int tempOrder = question.Order;
            question.Order = nextQuestion.Order;
            nextQuestion.Order = tempOrder;

            var idToken = LocalStorageHelper.GetItem("idToken");
            await _questionService.UpdateQuestionAsync(question, idToken);
            await _questionService.UpdateQuestionAsync(nextQuestion, idToken);
            await LoadQuestionsAsync();
        }

        private async Task ReorderQuestionsAsync()
        {
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                for (int i = 0; i < Questions.Count; i++)
                {
                    var question = Questions[i];
                    question.Order = i + 1;
                    await _questionService.UpdateQuestionAsync(question, idToken);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi sắp xếp lại câu hỏi: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
            }
        }
    }
}