using LanguageLearningApp.Models;
using LanguageLearningApp.ViewModels.Admin;

namespace LanguageLearningApp.Views.Admin
{
    public partial class ShopManagementPage : ContentPage
    {
        private AdminShopViewModel _viewModel;

        public ShopManagementPage(AdminShopViewModel viewModel)
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

        private void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                // The ViewModel will handle setting up the editing form
                _viewModel.SelectedItem = (ShopItemModel)e.CurrentSelection[0];

                // Reset selection
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}
