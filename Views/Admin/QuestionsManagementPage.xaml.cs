using LanguageLearningApp.Services;
using LanguageLearningApp.ViewModels.Admin;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Views.Admin
{
    public partial class QuestionsManagementPage : ContentPage
    {
        public QuestionsManagementPage()
        {
            InitializeComponent();
            BindingContext = new AdminQuestionViewModel(
                App.Services.GetService<QuestionService>(),
                App.Services.GetService<LessonService>(),
                App.Services.GetService<StorageService>()
            );
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AdminQuestionViewModel vm)
            {
                // Nếu LessonId đã truyền qua Route, sẽ tự động load dữ liệu
                if (!string.IsNullOrEmpty(vm.LessonId))
                    await vm.LoadLessonAndQuestionsAsync();
            }
        }
    }
}
