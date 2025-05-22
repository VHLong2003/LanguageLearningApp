using LanguageLearningApp.Models;
using LanguageLearningApp.ViewModels.Admin;

namespace LanguageLearningApp.Views.Admin
{
    public partial class BadgeManagementPage : ContentPage
    {
        private AdminBadgeViewModel _viewModel;

        public BadgeManagementPage(AdminBadgeViewModel viewModel)
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

        private void OnBadgeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                // The ViewModel will handle setting up the editing form
                _viewModel.SelectedBadge = (BadgeModel)e.CurrentSelection[0];

                // Reset selection
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}
