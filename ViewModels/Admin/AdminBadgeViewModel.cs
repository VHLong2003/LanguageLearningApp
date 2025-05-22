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
    public class AdminBadgeViewModel : BaseViewModel
    {
        private readonly BadgeService _badgeService;
        private readonly StorageService _storageService;

        private ObservableCollection<BadgeModel> _badges;
        private BadgeModel _selectedBadge;
        private bool _isRefreshing;
        private bool _isLoading;
        private bool _isEditing;
        private bool _isNewBadge;
        private string _errorMessage;
        private ObservableCollection<BadgeTier> _badgeTiers;

        // Form fields for editing/creating badge
        private string _badgeId;
        private string _title;
        private string _description;
        private string _iconUrl;
        private string _criteria;
        private int _requiredValue;
        private int _pointsReward;
        private int _coinsReward;
        private BadgeTier _tier;
        private bool _isHidden;

        public ObservableCollection<BadgeModel> Badges
        {
            get => _badges;
            set => SetProperty(ref _badges, value);
        }

        public BadgeModel SelectedBadge
        {
            get => _selectedBadge;
            set
            {
                if (SetProperty(ref _selectedBadge, value) && value != null)
                {
                    // Update form fields with selected badge
                    BadgeId = value.BadgeId;
                    Title = value.Title;
                    Description = value.Description;
                    IconUrl = value.IconUrl;
                    Criteria = value.Criteria;
                    RequiredValue = value.RequiredValue;
                    PointsReward = value.PointsReward;
                    CoinsReward = value.CoinsReward;
                    Tier = value.Tier;
                    IsHidden = value.IsHidden;

                    IsEditing = true;
                    IsNewBadge = false;
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

        public bool IsNewBadge
        {
            get => _isNewBadge;
            set => SetProperty(ref _isNewBadge, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<BadgeTier> BadgeTiers
        {
            get => _badgeTiers;
            set => SetProperty(ref _badgeTiers, value);
        }

        // Form properties
        public string BadgeId
        {
            get => _badgeId;
            set => SetProperty(ref _badgeId, value);
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

        public string Criteria
        {
            get => _criteria;
            set => SetProperty(ref _criteria, value);
        }

        public int RequiredValue
        {
            get => _requiredValue;
            set => SetProperty(ref _requiredValue, value);
        }

        public int PointsReward
        {
            get => _pointsReward;
            set => SetProperty(ref _pointsReward, value);
        }

        public int CoinsReward
        {
            get => _coinsReward;
            set => SetProperty(ref _coinsReward, value);
        }

        public BadgeTier Tier
        {
            get => _tier;
            set => SetProperty(ref _tier, value);
        }

        public bool IsHidden
        {
            get => _isHidden;
            set => SetProperty(ref _isHidden, value);
        }

        // Common criteria types for selection
        public ObservableCollection<string> CriteriaTypes { get; } = new ObservableCollection<string>
        {
            "Chuỗi ngày đăng nhập liên tục",
            "Tổng điểm tích lũy",
            "Kết bạn mới",
            "Hoàn thành khóa học",
            "Hoàn thành số bài học",
            "Đạt điểm tuyệt đối",
            "Đăng nhập hằng ngày"
        };

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand CreateBadgeCommand { get; }
        public ICommand SaveBadgeCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteBadgeCommand { get; }
        public ICommand PickIconCommand { get; }

        public AdminBadgeViewModel(
            BadgeService badgeService,
            StorageService storageService)
        {
            _badgeService = badgeService;
            _storageService = storageService;

            Badges = new ObservableCollection<BadgeModel>();

            // Available badge tiers
            BadgeTiers = new ObservableCollection<BadgeTier>
            {
                BadgeTier.Bronze,
                BadgeTier.Silver,
                BadgeTier.Gold,
                BadgeTier.Platinum,
                BadgeTier.Diamond
            };

            RefreshCommand = new Command(async () => await LoadBadgesAsync());
            CreateBadgeCommand = new Command(CreateNewBadge);
            SaveBadgeCommand = new Command(async () => await SaveBadgeAsync());
            CancelEditCommand = new Command(CancelEdit);
            DeleteBadgeCommand = new Command<BadgeModel>(async (badge) => await DeleteBadgeAsync(badge));
            PickIconCommand = new Command(async () => await PickIconAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadBadgesAsync();
        }

        private async Task LoadBadgesAsync()
        {
            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var allBadges = await _badgeService.GetAllBadgesAsync(idToken);

                // Sort by tier (Bronze, Silver, Gold, etc.)
                allBadges.Sort((a, b) => a.Tier.CompareTo(b.Tier));

                Badges.Clear();
                foreach (var badge in allBadges)
                {
                    Badges.Add(badge);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading badges: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void CreateNewBadge()
        {
            // Clear form fields
            BadgeId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            IconUrl = string.Empty;
            Criteria = "streak"; // Default
            RequiredValue = 7; // Default (e.g. 7-day streak)
            PointsReward = 50; // Default
            CoinsReward = 10; // Default
            Tier = BadgeTier.Bronze; // Default
            IsHidden = false;

            IsNewBadge = true;
            IsEditing = true;
            SelectedBadge = null;
            ErrorMessage = string.Empty;
        }

        private async Task SaveBadgeAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Description is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Criteria))
            {
                ErrorMessage = "Criteria is required";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                if (IsNewBadge)
                {
                    // Create new badge
                    var newBadge = new BadgeModel
                    {
                        Title = Title,
                        Description = Description,
                        IconUrl = IconUrl,
                        Criteria = Criteria,
                        RequiredValue = RequiredValue,
                        PointsReward = PointsReward,
                        CoinsReward = CoinsReward,
                        Tier = Tier,
                        IsHidden = IsHidden
                    };

                    var badgeId = await _badgeService.CreateBadgeAsync(newBadge, idToken);

                    if (!string.IsNullOrEmpty(badgeId))
                    {
                        newBadge.BadgeId = badgeId;
                        Badges.Add(newBadge);

                        // Reset form
                        CancelEdit();
                    }
                    else
                    {
                        ErrorMessage = "Failed to create badge";
                    }
                }
                else
                {
                    // Update existing badge
                    if (_selectedBadge != null)
                    {
                        _selectedBadge.Title = Title;
                        _selectedBadge.Description = Description;
                        _selectedBadge.IconUrl = IconUrl;
                        _selectedBadge.Criteria = Criteria;
                        _selectedBadge.RequiredValue = RequiredValue;
                        _selectedBadge.PointsReward = PointsReward;
                        _selectedBadge.CoinsReward = CoinsReward;
                        _selectedBadge.Tier = Tier;
                        _selectedBadge.IsHidden = IsHidden;

                        var success = await _badgeService.UpdateBadgeAsync(_selectedBadge, idToken);

                        if (success)
                        {
                            // Reset form
                            CancelEdit();

                            // Reload badges
                            await LoadBadgesAsync();
                        }
                        else
                        {
                            ErrorMessage = "Failed to update badge";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving badge: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            IsNewBadge = false;
            SelectedBadge = null;

            // Clear form fields
            BadgeId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            IconUrl = string.Empty;
            Criteria = string.Empty;
            RequiredValue = 0;
            PointsReward = 0;
            CoinsReward = 0;
            Tier = BadgeTier.Bronze;
            IsHidden = false;

            ErrorMessage = string.Empty;
        }

        private async Task DeleteBadgeAsync(BadgeModel badge)
        {
            if (badge == null)
                return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete the badge '{badge.Title}'?",
                "Yes", "No");

            if (!confirm)
                return;

            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var success = await _badgeService.DeleteBadgeAsync(badge.BadgeId, idToken);

                if (success)
                {
                    Badges.Remove(badge);

                    // If the deleted badge was being edited, clear the form
                    if (SelectedBadge == badge)
                    {
                        CancelEdit();
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to delete badge", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error deleting badge: {ex.Message}", "OK");
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
                // Use MAUI file picker
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select Badge Icon"
                });

                if (result != null)
                {
                    IsLoading = true;

                    var idToken = LocalStorageHelper.GetItem("idToken");
                    var iconUrl = await _storageService.UploadImageFromPickerAsync(result, "badge_icons", idToken);

                    if (!string.IsNullOrEmpty(iconUrl))
                    {
                        IconUrl = iconUrl;
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Failed to upload badge icon", "OK");
                    }

                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error picking icon: {ex.Message}", "OK");
            }
        }
    }
}
