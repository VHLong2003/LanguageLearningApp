using LanguageLearningApp.ViewModels.Admin;

namespace LanguageLearningApp.Views.Admin
{
    public partial class QuestionsManagementPage : ContentPage
    {
        public QuestionsManagementPage(string lessonId)
        {
            InitializeComponent();
            BindingContext = new AdminQuestionViewModel(lessonId);
        }
    }
}
