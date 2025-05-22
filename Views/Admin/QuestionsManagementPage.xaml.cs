using LanguageLearningApp.Models;
using LanguageLearningApp.ViewModels.Admin;

namespace LanguageLearningApp.Views.Admin
{
    public partial class QuestionsManagementPage : ContentPage
    {
        private AdminQuestionViewModel _viewModel;

        public QuestionsManagementPage(AdminQuestionViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private void OnQuestionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                // The ViewModel will handle setting up the editing form
                _viewModel.SelectedQuestion = (QuestionModel)e.CurrentSelection[0];

                // Reset selection
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void OnCorrectAnswerSelected(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked)
            {
                // Get the data context of the parent Grid
                if (radioButton.Parent is Grid grid && grid.BindingContext != null)
                {
                    // Set the correct answer in the view model
                    var viewModel = this.BindingContext as AdminQuestionViewModel;
                    viewModel.CorrectAnswer = grid.BindingContext.ToString();
                }
            }
        }

    }
}
