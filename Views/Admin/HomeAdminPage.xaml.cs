using LanguageLearningApp.ViewModels.Admin;

namespace LanguageLearningApp.Views.Admin
{
    public partial class HomeAdminPage : ContentPage
    {
        private AdminHomeViewModel _viewModel;

        public HomeAdminPage(AdminHomeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
