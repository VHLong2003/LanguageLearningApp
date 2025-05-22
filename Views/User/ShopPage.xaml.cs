using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    public partial class ShopPage : ContentPage
    {
        private ShopViewModel _viewModel;

        public ShopPage(ShopViewModel viewModel)
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

        private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                // Execute the ItemSelectedCommand
                _viewModel.ItemSelectedCommand.Execute(e.CurrentSelection[0]);

                // Reset selection
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}
