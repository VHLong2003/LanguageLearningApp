using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.User
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly ProgressService _progressService;
        private readonly BadgeService _badgeService;
        private readonly StorageService _storageService;
        private readonly AuthService _authService;

        private UsersModel _user;
        private ObservableCollection<BadgeModel> _badges;
        private ObservableCollection<ProgressModel> _recentProgress;
        private bool _isLoading;
        private bool _isRefreshing;
        private bool _isEditing;
        private string _statusMessage;
        private bool _isViewingOtherUser;
        private string _viewedUserId;

        // Edit profile fields
        private string _fullName;
        private string _email;
        private string _avatarUrl;
        private string _oldPassword;
        private string _newPassword;
        private string _confirmPassword;

        public UsersModel User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public ObservableCollection<BadgeModel> Badges
        {
            get => _badges;
            set => SetProperty(ref _badges, value);
        }

        public ObservableCollection<ProgressModel> RecentProgress
        {
            get => _recentProgress;
            set => SetProperty(ref _recentProgress, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsViewingOtherUser
        {
            get => _isViewingOtherUser;
            set => SetProperty(ref _isViewingOtherUser, value);
        }

        // Edit profile properties
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        public string OldPassword
        {
            get => _oldPassword;
            set => SetProperty(ref _oldPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ChangeAvatarCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand AddFriendCommand { get; }

        public ProfileViewModel(
            UserService userService,
            ProgressService progressService,
            BadgeService badgeService,
            StorageService storageService,
            AuthService authService)
        {
            _userService = userService;
            _progressService = progressService;
            _badgeService = badgeService;
            _storageService = storageService;
            _authService = authService;

            Badges = new ObservableCollection<BadgeModel>();
            RecentProgress = new ObservableCollection<ProgressModel>();

            RefreshCommand = new Command(async () => await LoadProfileDataAsync());
            EditProfileCommand = new Command(StartEditProfile);
            SaveProfileCommand = new Command(async () => await SaveProfileAsync());
            CancelEditCommand = new Command(CancelEdit);
            ChangeAvatarCommand = new Command(async () => await ChangeAvatarAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            AddFriendCommand = new Command(async () => await AddFriendAsync());
        }

        public async Task InitializeAsync(string userId = null)
        {
            _viewedUserId = userId;
            IsViewingOtherUser = !string.IsNullOrEmpty(userId);

            await LoadProfileDataAsync();
        }

        private async Task LoadProfileDataAsync()
        {
            IsLoading = true;
            IsRefreshing = true;
            StatusMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var currentUserId = LocalStorageHelper.GetItem("userId");

                // Determine which user to load
                string userIdToLoad = IsViewingOtherUser ? _viewedUserId : currentUserId;

                // Load user data
                var user = await _userService.GetUserByIdAsync(userIdToLoad, idToken);

                if (user != null)
                {
                    User = user;

                    // Update form fields if this is the current user
                    if (!IsViewingOtherUser)
                    {
                        FullName = user.FullName;
                        Email = user.Email;
                        AvatarUrl = user.AvatarUrl;
                    }

                    // Load badges
                    var userBadges = await _badgeService.GetUserBadgesAsync(userIdToLoad, idToken);

                    Badges.Clear();
                    foreach (var badge in userBadges)
                    {
                        Badges.Add(badge);
                    }

                    // Load recent progress
                    var allProgress = await _progressService.GetUserProgressAsync(userIdToLoad, idToken);

                    // Sort by completion date (most recent first) and take top 5
                    allProgress.Sort((a, b) => b.CompletedDate.CompareTo(a.CompletedDate));
                    var recentItems = allProgress.Count > 5 ? allProgress.GetRange(0, 5) : allProgress;

                    RecentProgress.Clear();
                    foreach (var progress in recentItems)
                    {
                        RecentProgress.Add(progress);
                    }
                }
                else
                {
                    StatusMessage = "Failed to load user data";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        private void StartEditProfile()
        {
            // Only allow editing if viewing own profile
            if (IsViewingOtherUser)
                return;

            IsEditing = true;

            // Clear password fields
            OldPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        private async Task SaveProfileAsync()
        {
            // Only allow saving if viewing own profile
            if (IsViewingOtherUser)
                return;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(FullName))
            {
                StatusMessage = "Please enter your full name";
                return;
            }

            // Validate password if changing
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (string.IsNullOrWhiteSpace(OldPassword))
                {
                    StatusMessage = "Please enter your current password";
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    StatusMessage = "New passwords don't match";
                    return;
                }

                if (!ValidationHelper.IsValidPassword(NewPassword))
                {
                    StatusMessage = "New password must be at least 6 characters";
                    return;
                }

                // TODO: Implement password change through Firebase Auth
            }

            IsLoading = true;
            StatusMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Update user data
                User.FullName = FullName;
                User.AvatarUrl = AvatarUrl;

                var success = await _userService.UpdateUserAsync(User, idToken);

                if (success)
                {
                    IsEditing = false;
                    StatusMessage = "Profile updated successfully";
                }
                else
                {
                    StatusMessage = "Failed to update profile";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;

            // Reset form fields to current values
            FullName = User.FullName;
            Email = User.Email;
            AvatarUrl = User.AvatarUrl;

            // Clear password fields
            OldPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;

            StatusMessage = string.Empty;
        }

        private async Task ChangeAvatarAsync()
        {
            try
            {
                // Use MAUI file picker
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select Profile Picture"
                });

                if (result != null)
                {
                    IsLoading = true;

                    var idToken = LocalStorageHelper.GetItem("idToken");
                    var userId = LocalStorageHelper.GetItem("userId");

                    // Upload image
                    var imageUrl = await _storageService.UploadImageFromPickerAsync(result, "avatars", idToken);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Update avatar URL
                        AvatarUrl = imageUrl;
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Failed to upload image", "OK");
                    }

                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error selecting image: {ex.Message}", "OK");
            }
        }

        private async Task LogoutAsync()
        {
            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm Logout",
                "Are you sure you want to log out?",
                "Yes", "No");

            if (!confirm)
                return;

            IsLoading = true;

            try
            {
                var success = await _authService.LogoutAsync();

                if (success)
                {
                    // Navigate to login
                    await Shell.Current.GoToAsync("//login");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error logging out: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddFriendAsync()
        {
            // Only allow adding friend when viewing other user
            if (!IsViewingOtherUser || string.IsNullOrEmpty(_viewedUserId))
                return;

            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var currentUserId = LocalStorageHelper.GetItem("userId");

                // Add friend (bidirectional relationship)
                var addToCurrentUser = await _userService.AddFriendAsync(currentUserId, _viewedUserId, idToken);
                var addToOtherUser = await _userService.AddFriendAsync(_viewedUserId, currentUserId, idToken);

                if (addToCurrentUser && addToOtherUser)
                {
                    await App.Current.MainPage.DisplayAlert("Success", "Friend added successfully", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to add friend", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error adding friend: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
