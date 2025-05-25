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

            // Khởi tạo danh mục
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

                // Lấy dữ liệu người dùng hiện tại
                _currentUser = await _userService.GetUserByIdAsync(userId, idToken);

                if (_currentUser != null)
                {
                    UserCoins = _currentUser.Coins;
                }

                // Tải các mặt hàng trong cửa hàng
                var items = await _shopService.GetAllShopItemsAsync(idToken);

                ShopItems.Clear();
                foreach (var item in items)
                {
                    // Kiểm tra xem người dùng đã sở hữu mặt hàng này chưa
                    if (_currentUser?.PurchasedItemIds == null || !_currentUser.PurchasedItemIds.Contains(item.ItemId))
                    {
                        // Chỉ hiển thị các mặt hàng người dùng chưa sở hữu và còn có sẵn
                        if (!item.IsLimited || item.AvailableQuantity > 0)
                        {
                            ShopItems.Add(item);
                        }
                    }
                }

                // Tải các mặt hàng của người dùng
                var userItems = await _shopService.GetUserPurchasedItemsAsync(userId, idToken);

                UserItems.Clear();
                foreach (var item in userItems)
                {
                    UserItems.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải cửa hàng: {ex.Message}", "OK");
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

                    // Lọc ra các mặt hàng người dùng chưa sở hữu
                    if (_currentUser?.PurchasedItemIds != null)
                    {
                        items = items.FindAll(i => !_currentUser.PurchasedItemIds.Contains(i.ItemId));
                    }

                    // Cập nhật trên luồng giao diện
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ShopItems.Clear();
                        foreach (var item in items)
                        {
                            // Chỉ hiển thị các mặt hàng có sẵn
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
                // Kiểm tra xem người dùng có đủ xu không
                if (_currentUser.Coins < item.Price)
                {
                    await App.Current.MainPage.DisplayAlert("Mua thất bại",
                        $"Bạn không có đủ xu. Bạn cần {item.Price} xu, nhưng bạn chỉ có {_currentUser.Coins} xu.",
                        "OK");
                    return;
                }

                // Xác nhận mua
                var confirm = await App.Current.MainPage.DisplayAlert("Xác nhận mua",
                    $"Bạn có chắc muốn mua {item.Title} với giá {item.Price} xu không?",
                    "Có", "Không");

                if (!confirm) return;

                // Xử lý giao dịch mua
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                var success = await _shopService.PurchaseItemAsync(userId, item.ItemId, idToken);

                if (success)
                {
                    // Cập nhật giao diện
                    _currentUser.Coins -= item.Price;
                    UserCoins = _currentUser.Coins;

                    // Làm mới cửa hàng
                    await LoadShopAsync();

                    await App.Current.MainPage.DisplayAlert("Mua thành công",
                        $"Bạn đã mua {item.Title}!",
                        "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Mua thất bại",
                        "Đã xảy ra lỗi khi xử lý giao dịch của bạn. Vui lòng thử lại.",
                        "OK");
                }
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi",
                    $"Đã xảy ra lỗi: {ex.Message}",
                    "OK");
            }
        }

        private async Task UseItemAsync(ShopItemModel item)
        {
            if (item == null) return;

            try
            {
                // Xử lý việc sử dụng mặt hàng dựa trên loại
                switch (item.Type)
                {
                    case ItemType.Avatar:
                        // Cập nhật ảnh đại diện của người dùng
                        _currentUser.AvatarUrl = item.IconUrl;

                        var idToken = LocalStorageHelper.GetItem("idToken");
                        await _userService.UpdateUserAsync(_currentUser, idToken);

                        await App.Current.MainPage.DisplayAlert("Cập nhật ảnh đại diện",
                            $"Ảnh đại diện của bạn đã được cập nhật thành {item.Title}!",
                            "OK");
                        break;

                    case ItemType.Theme:
                        // Áp dụng chủ đề
                        // Điều này sẽ yêu cầu một dịch vụ chủ đề và áp dụng các chủ đề
                        await App.Current.MainPage.DisplayAlert("Áp dụng chủ đề",
                            $"Chủ đề {item.Title} đã được áp dụng!",
                            "OK");
                        break;

                    case ItemType.PowerUp:
                        // Áp dụng hiệu ứng tăng cường
                        await App.Current.MainPage.DisplayAlert("Kích hoạt tăng cường",
                            $"{item.Title} đã được kích hoạt! {item.EffectDescription}",
                            "OK");
                        break;

                    default:
                        await App.Current.MainPage.DisplayAlert("Sử dụng mặt hàng",
                            $"Bạn đã sử dụng {item.Title}!",
                            "OK");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi",
                    $"Đã xảy ra lỗi: {ex.Message}",
                    "OK");
            }
        }
    }
}