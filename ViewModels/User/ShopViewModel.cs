using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.User
{
    public class ShopViewModel : BaseViewModel
    {
        private readonly ShopService _shopService;
        private readonly UserService _userService;
        private readonly StorageService _storageService;

        private ObservableCollection<ShopItemModel> _shopItems;
        private ObservableCollection<ShopItemModel> _userItems;
        private ObservableCollection<ItemType> _categories;
        private ItemType _selectedCategory;
        private bool _isRefreshing;
        private bool _showingUserItems;
        private UsersModel _currentUser;
        private int _userCoins;
        private ShopItemModel _selectedItem;

        public ObservableCollection<ShopItemModel> ShopItems
        {
            get => _shopItems;
            set => SetProperty(ref _shopItems, value);
        }

        public ObservableCollection<ShopItemModel> UserItems
        {
            get => _userItems;
            set => SetProperty(ref _userItems, value);
        }

        public ObservableCollection<ItemType> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ItemType SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterShopItems();
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool ShowingUserItems
        {
            get => _showingUserItems;
            set => SetProperty(ref _showingUserItems, value);
        }

        public int UserCoins
        {
            get => _userCoins;
            set => SetProperty(ref _userCoins, value);
        }

        public ShopItemModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ShowShopCommand { get; }
        public ICommand ShowInventoryCommand { get; }
        public ICommand PurchaseItemCommand { get; }
        public ICommand ItemSelectedCommand { get; }
        public ICommand UseItemCommand { get; }

        public ShopViewModel(ShopService shopService, UserService userService, StorageService storageService)
        {
            _shopService = shopService;
            _userService = userService;
            _storageService = storageService;

            ShopItems = new ObservableCollection<ShopItemModel>();
            UserItems = new ObservableCollection<ShopItemModel>();

            // Initialize categories
            Categories = new ObservableCollection<ItemType>
            {
                ItemType.Avatar,
                ItemType.Theme,
                ItemType.PowerUp,
                ItemType.Customization,
                ItemType.Decoration,
                ItemType.Special
            };

            RefreshCommand = new Command(async () => await LoadShopAsync());
            ShowShopCommand = new Command(() => ShowingUserItems = false);
            ShowInventoryCommand = new Command(() => ShowingUserItems = true);
            PurchaseItemCommand = new Command<ShopItemModel>(async (item) => await PurchaseItemAsync(item));
            ItemSelectedCommand = new Command<ShopItemModel>(item => SelectedItem = item);
            UseItemCommand = new Command<ShopItemModel>(async (item) => await UseItemAsync(item));
        }

        public async Task InitializeAsync()
        {
            await LoadShopAsync();
        }

        private async Task LoadShopAsync()
        {
            IsRefreshing = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Get current user data
                _currentUser = await _userService.GetUserByIdAsync(userId, idToken);

                if (_currentUser != null)
                {
                    UserCoins = _currentUser.Coins;
                }

                // Load shop items
                var items = await _shopService.GetAllShopItemsAsync(idToken);

                ShopItems.Clear();
                foreach (var item in items)
                {
                    // Check if user already owns this item
                    if (_currentUser?.PurchasedItemIds == null || !_currentUser.PurchasedItemIds.Contains(item.ItemId))
                    {
                        // Only show items the user doesn't own yet and that are available
                        if (!item.IsLimited || item.AvailableQuantity > 0)
                        {
                            ShopItems.Add(item);
                        }
                    }
                }

                // Load user items
                var userItems = await _shopService.GetUserPurchasedItemsAsync(userId, idToken);

                UserItems.Clear();
                foreach (var item in userItems)
                {
                    UserItems.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load shop: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void FilterShopItems()
        {
            if (SelectedCategory == default)
                return;

            IsRefreshing = true;

            var idToken = LocalStorageHelper.GetItem("idToken");
            Task.Run(async () =>
            {
                try
                {
                    var items = await _shopService.GetItemsByTypeAsync(SelectedCategory, idToken);

                    // Filter out items the user already owns
                    if (_currentUser?.PurchasedItemIds != null)
                    {
                        items = items.FindAll(i => !_currentUser.PurchasedItemIds.Contains(i.ItemId));
                    }

                    // Update on UI thread
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ShopItems.Clear();
                        foreach (var item in items)
                        {
                            // Only show available items
                            if (!item.IsLimited || item.AvailableQuantity > 0)
                            {
                                ShopItems.Add(item);
                            }
                        }

                        IsRefreshing = false;
                    });
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsRefreshing = false;
                    });
                }
            });
        }

        private async Task PurchaseItemAsync(ShopItemModel item)
        {
            if (item == null) return;

            try
            {
                // Check if user has enough coins
                if (_currentUser.Coins < item.Price)
                {
                    await App.Current.MainPage.DisplayAlert("Purchase Failed",
                        $"You don't have enough coins. You need {item.Price} coins, but you only have {_currentUser.Coins} coins.",
                        "OK");
                    return;
                }

                // Confirm purchase
                var confirm = await App.Current.MainPage.DisplayAlert("Confirm Purchase",
                    $"Are you sure you want to purchase {item.Title} for {item.Price} coins?",
                    "Yes", "No");

                if (!confirm) return;

                // Process the purchase
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                var success = await _shopService.PurchaseItemAsync(userId, item.ItemId, idToken);

                if (success)
                {
                    // Update UI
                    _currentUser.Coins -= item.Price;
                    UserCoins = _currentUser.Coins;

                    // Refresh shop
                    await LoadShopAsync();

                    await App.Current.MainPage.DisplayAlert("Purchase Successful",
                        $"You have purchased {item.Title}!",
                        "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Purchase Failed",
                        "There was an error processing your purchase. Please try again.",
                        "OK");
                }
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error",
                    $"An error occurred: {ex.Message}",
                    "OK");
            }
        }

        private async Task UseItemAsync(ShopItemModel item)
        {
            if (item == null) return;

            try
            {
                // Handle item usage based on type
                switch (item.Type)
                {
                    case ItemType.Avatar:
                        // Update user's avatar
                        _currentUser.AvatarUrl = item.IconUrl;

                        var idToken = LocalStorageHelper.GetItem("idToken");
                        await _userService.UpdateUserAsync(_currentUser, idToken);

                        await App.Current.MainPage.DisplayAlert("Avatar Updated",
                            $"Your avatar has been updated to {item.Title}!",
                            "OK");
                        break;

                    case ItemType.Theme:
                        // Apply theme
                        // This would require a theme service and application of themes
                        await App.Current.MainPage.DisplayAlert("Theme Applied",
                            $"Theme {item.Title} has been applied!",
                            "OK");
                        break;

                    case ItemType.PowerUp:
                        // Apply power-up effect
                        await App.Current.MainPage.DisplayAlert("Power-Up Activated",
                            $"{item.Title} has been activated! {item.EffectDescription}",
                            "OK");
                        break;

                    default:
                        await App.Current.MainPage.DisplayAlert("Item Used",
                            $"You've used {item.Title}!",
                            "OK");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error",
                    $"An error occurred: {ex.Message}",
                    "OK");
            }
        }
    }
}
