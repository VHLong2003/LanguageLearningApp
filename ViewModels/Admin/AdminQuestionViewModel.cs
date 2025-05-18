using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace LanguageLearningApp.ViewModels.Admin
{
    public partial class AdminQuestionViewModel : ObservableObject
    {
        private readonly QuestionService _questionService = new QuestionService();

        [ObservableProperty] private ObservableCollection<Question> questions = new();
        [ObservableProperty] private Question selectedQuestion = new();
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private string lessonId;
        [ObservableProperty]
        private string dragPairsText;

        [ObservableProperty] private QuestionType selectedQuestionType;

        // MultipleChoice
        [ObservableProperty] private string option1;
        [ObservableProperty] private string option2;
        [ObservableProperty] private string option3;
        [ObservableProperty] private string option4;
        [ObservableProperty] private int correctAnswerIndex = -1;
        public ObservableCollection<string> CurrentOptions { get; set; } = new();

        // FillInTheBlank: dùng CorrectAnswer (Entry)

        // DragAndDrop nhiều cặp
        [ObservableProperty] private ObservableCollection<DragPair> dragPairs = new();

        [RelayCommand]
        public void AddDragPair() => DragPairs.Add(new DragPair());
        [RelayCommand]
        public void RemoveDragPair(DragPair pair)
        {
            if (DragPairs.Contains(pair)) DragPairs.Remove(pair);
        }

        // TrueFalse
        [ObservableProperty] private int trueFalseAnswerIndex = -1;
        public ObservableCollection<string> TrueFalseOptions { get; set; } = new() { "Đúng", "Sai" };

        public AdminQuestionViewModel(string lessonId)
        {
            LessonId = lessonId;
            _ = LoadQuestionsAsync();
            SelectedQuestion = new Question { LessonId = lessonId };
            SelectedQuestionType = SelectedQuestion.Type;
            DragPairs = new ObservableCollection<DragPair>();
        }

        partial void OnOption1Changed(string value) => RefreshOptions();
        partial void OnOption2Changed(string value) => RefreshOptions();
        partial void OnOption3Changed(string value) => RefreshOptions();
        partial void OnOption4Changed(string value) => RefreshOptions();

        partial void OnSelectedQuestionTypeChanged(QuestionType value)
        {
            if (SelectedQuestion != null)
            {
                SelectedQuestion.Type = value;
                ClearInputExceptContentAndPoints();
                if (value == QuestionType.DragAndDrop && DragPairs.Count == 0)
                    DragPairs.Add(new DragPair());
            }
        }

        private void RefreshOptions()
        {
            CurrentOptions.Clear();
            if (!string.IsNullOrWhiteSpace(Option1)) CurrentOptions.Add(Option1);
            if (!string.IsNullOrWhiteSpace(Option2)) CurrentOptions.Add(Option2);
            if (!string.IsNullOrWhiteSpace(Option3)) CurrentOptions.Add(Option3);
            if (!string.IsNullOrWhiteSpace(Option4)) CurrentOptions.Add(Option4);
            if (CorrectAnswerIndex >= CurrentOptions.Count) CorrectAnswerIndex = -1;
        }

        [RelayCommand]
        public async Task LoadQuestionsAsync()
        {
            IsBusy = true;
            ErrorMessage = "";
            try
            {
                var all = await _questionService.GetQuestionsByLessonIdAsync(LessonId);
                Questions = new ObservableCollection<Question>(all);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task AddOrUpdateQuestionAsync()
        {
            if (SelectedQuestion == null || string.IsNullOrWhiteSpace(SelectedQuestion.Content))
            {
                ErrorMessage = "Nội dung câu hỏi không được để trống!";
                return;
            }
            SelectedQuestion.LessonId = LessonId;
            SelectedQuestion.Type = SelectedQuestionType;

            switch (SelectedQuestion.Type)
            {
                case QuestionType.MultipleChoice:
                    SelectedQuestion.Options = CurrentOptions.ToList();
                    if (CorrectAnswerIndex < 0 || CorrectAnswerIndex >= CurrentOptions.Count)
                    {
                        ErrorMessage = "Chọn đáp án đúng!";
                        return;
                    }
                    SelectedQuestion.CorrectAnswer = CurrentOptions[CorrectAnswerIndex];
                    break;

                case QuestionType.FillInTheBlank:
                    SelectedQuestion.Options = null;
                    if (string.IsNullOrWhiteSpace(SelectedQuestion.CorrectAnswer))
                    {
                        ErrorMessage = "Nhập đáp án đúng!";
                        return;
                    }
                    break;

                case QuestionType.DragAndDrop:
                    SelectedQuestion.Options = new List<string>();
                    if (!string.IsNullOrWhiteSpace(DragPairsText))
                    {
                        var lines = DragPairsText.Split('\n', '\r')
                            .Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
                        foreach (var line in lines)
                        {
                            if (line.Contains(":"))
                                SelectedQuestion.Options.Add(line);
                        }
                    }
                    SelectedQuestion.CorrectAnswer = string.Join("|", SelectedQuestion.Options);
                    break;


                case QuestionType.TrueFalse:
                    SelectedQuestion.Options = TrueFalseOptions.ToList();
                    if (TrueFalseAnswerIndex < 0 || TrueFalseAnswerIndex > 1)
                    {
                        ErrorMessage = "Chọn đúng/sai!";
                        return;
                    }
                    SelectedQuestion.CorrectAnswer = TrueFalseOptions[TrueFalseAnswerIndex];
                    break;
            }

            IsBusy = true;
            try
            {
                await _questionService.AddOrUpdateQuestionAsync(SelectedQuestion);
                await LoadQuestionsAsync();
                ClearInput();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteQuestionAsync(Question question)
        {
            if (question == null) return;
            IsBusy = true;
            try
            {
                await _questionService.DeleteQuestionAsync(question.QuestionId);
                Questions.Remove(question);
                if (SelectedQuestion == question) ClearInput();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void EditQuestion(Question question)
        {
            if (question != null)
            {
                SelectedQuestion = new Question
                {
                    QuestionId = question.QuestionId,
                    LessonId = question.LessonId,
                    Content = question.Content,
                    Type = question.Type,
                    Options = question.Options != null ? new List<string>(question.Options) : new List<string>(),
                    CorrectAnswer = question.CorrectAnswer,
                    Points = question.Points
                };

                SelectedQuestionType = question.Type;

                Option1 = question.Options != null && question.Options.Count > 0 ? question.Options[0] : "";
                Option2 = question.Options != null && question.Options.Count > 1 ? question.Options[1] : "";
                Option3 = question.Options != null && question.Options.Count > 2 ? question.Options[2] : "";
                Option4 = question.Options != null && question.Options.Count > 3 ? question.Options[3] : "";

                // DragAndDrop nhiều cặp
                DragPairs.Clear();
                if (question.Type == QuestionType.DragAndDrop && question.Options != null)
                {
                    DragPairsText = string.Join("\n", question.Options);
                }
                else
                {
                    DragPairsText = "";
                }


                RefreshOptions();
                CorrectAnswerIndex = question.Type == QuestionType.MultipleChoice
                    ? CurrentOptions.IndexOf(question.CorrectAnswer ?? "")
                    : -1;
                TrueFalseAnswerIndex = question.Type == QuestionType.TrueFalse
                    ? TrueFalseOptions.IndexOf(question.CorrectAnswer ?? "")
                    : -1;
            }
        }

        private void ClearInput()
        {
            SelectedQuestion = new Question { LessonId = LessonId };
            Option1 = Option2 = Option3 = Option4 = "";
            DragPairsText = "";
            CorrectAnswerIndex = -1;
            TrueFalseAnswerIndex = -1;
            CurrentOptions.Clear();
            SelectedQuestionType = QuestionType.MultipleChoice;
        }

        private void ClearInputExceptContentAndPoints()
        {
            Option1 = Option2 = Option3 = Option4 = "";
            DragPairsText = "";
            CorrectAnswerIndex = -1;
            TrueFalseAnswerIndex = -1;
            CurrentOptions.Clear();
        }
    }
}
