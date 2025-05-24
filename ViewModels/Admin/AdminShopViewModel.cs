using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.Admin
{
    public class AdminShopViewModel : BaseViewModel
    {
        private readonly ShopService _shopService;
        private readonly StorageService _storageService;

        private ObservableCollection<ShopItemModel> _shopItems;
        private ShopItemModel _selectedItem;
        private bool _isRefreshing;
        private bool _isLoading;
        private bool _isEditing;
        private bool _isNewItem;
        private string _errorMessage;
        private ObservableCollection<ItemType> _itemTypes;
        private ItemType _selectedType;

        private string _itemId;
        private string _title;
        private string _description;
        private string _iconUrl;
        private int _price;
        private ItemType _type;
        private bool _isLimited;
        private int _availableQuantity;
        private int _requiredLevel;
        private string _effectDescription;
        private int _durationDays;

        public ObservableCollection<ShopItemModel> ShopItems
        {
            get => _shopItems;
            set => SetProperty(ref _shopItems, value);
        }

        public ShopItemModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    ItemId = value.ItemId;
                    Title = value.Title;
                    Description = value.Description;
                    IconUrl = value.IconUrl;
                    Price = value.Price;
                    Type = value.Type;
                    IsLimited = value.IsLimited;
                    AvailableQuantity = value.AvailableQuantity;
                    RequiredLevel = value.RequiredLevel;
                    EffectDescription = value.EffectDescription;
                    DurationDays = value.DurationDays;

                    IsEditing = true;
                    IsNewItem = false;
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsNewItem
        {
            get => _isNewItem;
            set => SetProperty(ref _isNewItem, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<ItemType> ItemTypes
        {
            get => _itemTypes;
            set => SetProperty(ref _itemTypes, value);
        }

        public ItemType SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value) && value != ItemType.Special)
                {
                    FilterByType(value);
                }
            }
        }

        public string ItemId
        {
            get => _itemId;
            set => SetProperty(ref _itemId, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string IconUrl
        {
            get => _iconUrl;
            set => SetProperty(ref _iconUrl, value);
        }

        public int Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public ItemType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public bool IsLimited
        {
            get => _isLimited;
            set => SetProperty(ref _isLimited, value);
        }

        public int AvailableQuantity
        {
            get => _availableQuantity;
            set => SetProperty(ref _availableQuantity, value);
        }

        public int RequiredLevel
        {
            get => _requiredLevel;
            set => SetProperty(ref _requiredLevel, value);
        }

        public string EffectDescription
        {
            get => _effectDescription;
            set => SetProperty(ref _effectDescription, value);
        }

        public int DurationDays
        {
            get => _durationDays;
            set => SetProperty(ref _durationDays, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand CreateItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand PickIconCommand { get; }
        public ICommand FilterByTypeCommand { get; }

        public AdminShopViewModel(ShopService shopService, StorageService storageService)
        {
            _shopService = shopService;
            _storageService = storageService;

            ShopItems = new ObservableCollection<ShopItemModel>();

            ItemTypes = new ObservableCollection<ItemType>
            {
                ItemType.Avatar,
                ItemType.Theme,
                ItemType.PowerUp,
                ItemType.Customization,
                ItemType.Decoration,
                ItemType.Special
            };

            RefreshCommand = new Command(async () => await LoadAllShopItemsAsync());
            CreateItemCommand = new Command(CreateNewItem);
            SaveItemCommand = new Command(async () => await SaveItemAsync());
            CancelEditCommand = new Command(CancelEdit);
            DeleteItemCommand = new Command<ShopItemModel>(async (item) => await DeleteItemAsync(item));
            PickIconCommand = new Command(async () => await PickIconAsync());
            FilterByTypeCommand = new Command<ItemType>(FilterByType);
        }

        public async Task InitializeAsync()
        {
            await LoadAllShopItemsAsync();
        }

        private async Task LoadAllShopItemsAsync()
        {
            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (string.IsNullOrEmpty(idToken))
                    throw new InvalidOperationException("User is not authenticated.");

                var items = await _shopService.GetAllShopItemsAsync(idToken);
                ShopItems.Clear();
                foreach (var item in items)
                {
                    ShopItems.Add(item);
                }
                SelectedType = default;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading shop items: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async void FilterByType(ItemType type)
        {
            if (type == default)
            {
                await LoadAllShopItemsAsync();
                return;
            }

            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var items = await _shopService.GetItemsByTypeAsync(type, idToken);
                ShopItems.Clear();
                foreach (var item in items)
                {
                    ShopItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error filtering shop items: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void CreateNewItem()
        {
            ItemId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            IconUrl = string.Empty;
            Price = 100;
            Type = ItemType.Avatar;
            IsLimited = false;
            AvailableQuantity = 0;
            RequiredLevel = 1;
            EffectDescription = string.Empty;
            DurationDays = 0;

            IsNewItem = true;
            IsEditing = true;
            SelectedItem = null;
            ErrorMessage = string.Empty;
        }

        private async Task SaveItemAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Description is required.";
                return;
            }

            if (Price < 0)
            {
                ErrorMessage = "Price must be non-negative.";
                return;
            }

            if (RequiredLevel < 0)
            {
                ErrorMessage = "Required level cannot be negative.";
                return;
            }

            if (IsLimited && AvailableQuantity < 0)
            {
                ErrorMessage = "Available quantity cannot be negative.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                if (IsNewItem)
                {
                    var newItem = new ShopItemModel
                    {
                        Title = Title,
                        Description = Description,
                        IconUrl = IconUrl,
                        Price = Price,
                        Type = Type,
                        IsLimited = IsLimited,
                        AvailableQuantity = IsLimited ? AvailableQuantity : 0,
                        RequiredLevel = RequiredLevel,
                        EffectDescription = EffectDescription,
                        DurationDays = DurationDays
                    };

                    var itemId = await _shopService.CreateShopItemAsync(newItem, idToken);
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        newItem.ItemId = itemId;
                        ShopItems.Add(newItem);
                        CancelEdit();
                    }
                    else
                    {
                        ErrorMessage = "Failed to create shop item.";
                    }
                }
                else
                {
                    if (SelectedItem != null)
                    {
                        SelectedItem.Title = Title;
                        SelectedItem.Description = Description;
                        SelectedItem.IconUrl = IconUrl;
                        SelectedItem.Price = Price;
                        SelectedItem.Type = Type;
                        SelectedItem.IsLimited = IsLimited;
                        SelectedItem.AvailableQuantity = IsLimited ? AvailableQuantity : 0;
                        SelectedItem.RequiredLevel = RequiredLevel;
                        SelectedItem.EffectDescription = EffectDescription;
                        SelectedItem.DurationDays = DurationDays;

                        var success = await _shopService.UpdateShopItemAsync(SelectedItem, idToken);
                        if (success)
                        {
                            CancelEdit();
                            if (SelectedType != default)
                                FilterByType(SelectedType);
                            else
                                await LoadAllShopItemsAsync();
                        }
                        else
                        {
                            ErrorMessage = "Failed to update shop item.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving shop item: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            IsNewItem = false;
            SelectedItem = null;

            ItemId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            IconUrl = string.Empty;
            Price = 0;
            Type = ItemType.Avatar;
            IsLimited = false;
            AvailableQuantity = 0;
            RequiredLevel = 0;
            EffectDescription = string.Empty;
            DurationDays = 0;

            ErrorMessage = string.Empty;
        }

        private async Task DeleteItemAsync(ShopItemModel item)
        {
            if (item == null) return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete the item '{item.Title}'?",
                "Yes", "No");

            if (!confirm) return;

            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                if (!string.IsNullOrEmpty(item.IconUrl))
                {
                    await _storageService.DeleteImageAsync(item.IconUrl, idToken);
                }

                var success = await _shopService.DeleteShopItemAsync(item.ItemId, idToken);
                if (success)
                {
                    ShopItems.Remove(item);
                    if (SelectedItem == item)
                    {
                        CancelEdit();
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to delete shop item.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error deleting shop item: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PickIconAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select Item Icon"
                });

                if (result != null)
                {
                    IsLoading = true;
                    var idToken = LocalStorageHelper.GetItem("idToken");
                    if (string.IsNullOrEmpty(idToken))
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "User is not authenticated. Please log in again.", "OK");
                        return;
                    }

                    var iconUrl = await _storageService.UploadImageFromPickerAsync(result, "shop_icons", idToken);
                    if (!string.IsNullOrEmpty(iconUrl))
                    {
                        IconUrl = iconUrl;
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Failed to upload item icon. Please check your internet connection or try again.", "OK");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                var message = ex.Message.Contains("403") ? "Access denied: Please check Firebase Storage rules or your authentication token."
                    : ex.Message.Contains("401") ? "Authentication failed: Please log in again."
                    : $"Error uploading icon: {ex.Message}";
                await App.Current.MainPage.DisplayAlert("Error", message, "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error picking icon: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}