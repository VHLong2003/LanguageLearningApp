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
                    // When lessonId is set via QueryProperty, load data
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
                    // Update form fields with selected question
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

                    // Load options
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

        // Form properties
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

        // Commands
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

            // Available question types
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
            BackToLessonsCommand = new Command(async () => await Shell.Current.GoToAsync($"../lessons?courseId={CourseId}"));
        }

        private async Task LoadLessonAndQuestionsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Load lesson details
                CurrentLesson = await _lessonService.GetLessonByIdAsync(LessonId, idToken);

                if (CurrentLesson != null)
                {
                    await LoadQuestionsAsync();
                }
                else
                {
                    ErrorMessage = "Lesson not found";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading lesson: {ex.Message}";
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
                var lessonQuestions = await _questionService.GetQuestionsByLessonIdAsync(LessonId, idToken);

                // Sort questions by order
                lessonQuestions.Sort((a, b) => a.Order.CompareTo(b.Order));

                Questions.Clear();
                foreach (var question in lessonQuestions)
                {
                    Questions.Add(question);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading questions: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void CreateNewQuestion()
        {
            // Clear form fields
            QuestionId = string.Empty;
            Content = string.Empty;
            ImageUrl = string.Empty;
            AudioUrl = string.Empty;
            Type = QuestionType.MultipleChoice; // Default
            CorrectAnswer = string.Empty;
            Explanation = string.Empty;
            Points = 10; // Default
            Order = Questions.Count + 1; // Set order to next in sequence
            TimeLimit = 30; // Default 30 seconds

            // Clear options
            Options.Clear();
            Options.Add("Option 1");
            Options.Add("Option 2");
            Options.Add("Option 3");
            Options.Add("Option 4");

            IsNewQuestion = true;
            IsEditing = true;
            SelectedQuestion = null;
            ErrorMessage = string.Empty;
        }

        private async Task SaveQuestionAsync()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                ErrorMessage = "Question content is required";
                return;
            }

            if (Type == QuestionType.MultipleChoice && Options.Count < 2)
            {
                ErrorMessage = "Multiple choice questions require at least 2 options";
                return;
            }

            if (string.IsNullOrWhiteSpace(CorrectAnswer))
            {
                ErrorMessage = "Correct answer is required";
                return;
            }

            if (Points <= 0)
            {
                ErrorMessage = "Points must be greater than 0";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                if (IsNewQuestion)
                {
                    // Create new question
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

                        // Reset form
                        CancelEdit();
                    }
                    else
                    {
                        ErrorMessage = "Failed to create question";
                    }
                }
                else
                {
                    // Update existing question
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
                            // Reset form
                            CancelEdit();

                            // Reload to ensure correct ordering
                            await LoadQuestionsAsync();
                        }
                        else
                        {
                            ErrorMessage = "Failed to update question";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving question: {ex.Message}";
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

            // Clear form fields
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

            // Clear options
            Options.Clear();
            NewOption = string.Empty;

            ErrorMessage = string.Empty;
        }

        private async Task DeleteQuestionAsync(QuestionModel question)
        {
            if (question == null)
                return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete this question?",
                "Yes", "No");

            if (!confirm)
                return;

            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var success = await _questionService.DeleteQuestionAsync(question.QuestionId, idToken);

                if (success)
                {
                    Questions.Remove(question);

                    // Reorder remaining questions
                    await ReorderQuestionsAsync();

                    // If the deleted question was being edited, clear the form
                    if (SelectedQuestion == question)
                    {
                        CancelEdit();
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to delete question", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error deleting question: {ex.Message}", "OK");
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
                // Use MAUI file picker
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select Question Image"
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
                        await App.Current.MainPage.DisplayAlert("Error", "Failed to upload image", "OK");
                    }

                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error picking image: {ex.Message}", "OK");
            }
        }

        private void AddOption()
        {
            if (string.IsNullOrWhiteSpace(NewOption))
                return;

            Options.Add(NewOption);
            NewOption = string.Empty;
        }

        private void RemoveOption(string option)
        {
            if (string.IsNullOrWhiteSpace(option))
                return;

            Options.Remove(option);

            // If the correct answer was this option, clear it
            if (CorrectAnswer == option)
                CorrectAnswer = string.Empty;
        }

        private async Task MoveUpAsync(QuestionModel question)
        {
            if (question == null)
                return;

            var index = Questions.IndexOf(question);

            // Can't move up if already first
            if (index <= 0)
                return;

            // Swap with previous question
            var previousQuestion = Questions[index - 1];

            // Swap order values
            int tempOrder = question.Order;
            question.Order = previousQuestion.Order;
            previousQuestion.Order = tempOrder;

            // Update questions in database
            var idToken = LocalStorageHelper.GetItem("idToken");
            await _questionService.UpdateQuestionAsync(question, idToken);
            await _questionService.UpdateQuestionAsync(previousQuestion, idToken);

            // Reload questions to refresh order
            await LoadQuestionsAsync();
        }

        private async Task MoveDownAsync(QuestionModel question)
        {
            if (question == null)
                return;

            var index = Questions.IndexOf(question);

            // Can't move down if already last
            if (index >= Questions.Count - 1)
                return;

            // Swap with next question
            var nextQuestion = Questions[index + 1];

            // Swap order values
            int tempOrder = question.Order;
            question.Order = nextQuestion.Order;
            nextQuestion.Order = tempOrder;

            // Update questions in database
            var idToken = LocalStorageHelper.GetItem("idToken");
            await _questionService.UpdateQuestionAsync(question, idToken);
            await _questionService.UpdateQuestionAsync(nextQuestion, idToken);

            // Reload questions to refresh order
            await LoadQuestionsAsync();
        }

        private async Task ReorderQuestionsAsync()
        {
            // Reorder remaining questions
            var idToken = LocalStorageHelper.GetItem("idToken");

            for (int i = 0; i < Questions.Count; i++)
            {
                var question = Questions[i];
                question.Order = i + 1;
                await _questionService.UpdateQuestionAsync(question, idToken);
            }
        }
    }
}
