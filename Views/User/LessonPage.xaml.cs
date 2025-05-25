using LanguageLearningApp.ViewModels.User;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Views.User
{
    public partial class LessonPage : ContentPage
    {
        private readonly LessonViewModel _viewModel;

        public LessonPage(LessonViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Khi trang xuất hiện, nạp lại dữ liệu nếu chưa có
            if (_viewModel.Questions == null || _viewModel.Questions.Count == 0)
            {
                await _viewModel.InitializeAsync();
            }
        }
    }
}
