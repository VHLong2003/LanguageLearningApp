using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.Admin
{
    public class AdminUserViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly ProgressService _progressService;
        private readonly BadgeService _badgeService;

        private ObservableCollection<UsersModel> _users;
        private UsersModel _selectedUser;
        private bool _isRefreshing;
        private bool _isLoading;
        private bool _isEditing;
        private string _errorMessage;
        private string _searchQuery;
        private ObservableCollection<string> _roles;

        // Form fields for editing user
        private string _userId;
        private string _fullName;
        private string _email;
        private string _role;
        private int _points;
        private int _coins;
        private int _currentStreak;
        private string _avatarUrl;

        public ObservableCollection<UsersModel> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public UsersModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value) && value != null)
                {
                    // Update form fields with selected user
                    UserId = value.UserId;
                    FullName = value.FullName;
                    Email = value.Email;
                    Role = value.Role;
                    Points = value.Points;
                    Coins = value.Coins;
                    CurrentStreak = value.CurrentStreak;
                    AvatarUrl = value.AvatarUrl;

                    IsEditing = true;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    FilterUsers();
                }
            }
        }

        public ObservableCollection<string> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        // Form properties
        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

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

        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        public int Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        public int Coins
        {
            get => _coins;
            set => SetProperty(ref _coins, value);
        }

        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }

        public string AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        // Commands
        public ICommand LoadUsersCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ViewUserProgressCommand { get; }
        public ICommand ViewUserBadgesCommand { get; }
        public ICommand ManageFriendsCommand { get; }

        public AdminUserViewModel(
            UserService userService,
            ProgressService progressService,
            BadgeService badgeService)
        {
            _userService = userService;
            _progressService = progressService;
            _badgeService = badgeService;

            Users = new ObservableCollection<UsersModel>();

            // Available roles
            Roles = new ObservableCollection<string> { "User", "Admin" };

            LoadUsersCommand = new Command(async () => await LoadUsersAsync());
            SaveUserCommand = new Command(async () => await SaveUserAsync());
            CancelEditCommand = new Command(CancelEdit);
            DeleteUserCommand = new Command<UsersModel>(async (user) => await DeleteUserAsync(user));
            ViewUserProgressCommand = new Command<UsersModel>(async (user) => await ViewUserProgressAsync(user));
            ViewUserBadgesCommand = new Command<UsersModel>(async (user) => await ViewUserBadgesAsync(user));
            ManageFriendsCommand = new Command<UsersModel>(async (user) => await ManageFriendsAsync(user));
        }

        public async Task InitializeAsync()
        {
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var allUsers = await _userService.GetAllUsersAsync(idToken);

                // Sort by join date (newest first)
                allUsers.Sort((a, b) => b.DateJoined.CompareTo(a.DateJoined));

                Users.Clear();
                foreach (var user in allUsers)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading users: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            var query = SearchQuery.ToLower();

            IsRefreshing = true;

            var idToken = LocalStorageHelper.GetItem("idToken");
            Task.Run(async () =>
            {
                try
                {
                    var allUsers = await _userService.GetAllUsersAsync(idToken);

                    // Filter by name, email, or user ID
                    var filteredUsers = allUsers.Where(u =>
                        u.FullName.ToLower().Contains(query) ||
                        u.Email.ToLower().Contains(query) ||
                        u.UserId.ToLower().Contains(query)).ToList();

                    // Sort by join date (newest first)
                    filteredUsers.Sort((a, b) => b.DateJoined.CompareTo(a.DateJoined));

                    // Update on UI thread
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Users.Clear();
                        foreach (var user in filteredUsers)
                        {
                            Users.Add(user);
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

        private async Task SaveUserAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Full name is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Role))
            {
                ErrorMessage = "Role is required";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Update the user
                var user = await _userService.GetUserByIdAsync(UserId, idToken);

                if (user != null)
                {
                    user.FullName = FullName;
                    user.Role = Role;
                    user.Points = Points;
                    user.Coins = Coins;
                    user.CurrentStreak = CurrentStreak;
                    user.AvatarUrl = AvatarUrl;

                    var success = await _userService.UpdateUserAsync(user, idToken);

                    if (success)
                    {
                        await LoadUsersAsync();
                        IsEditing = false;

                        // Show success message
                        await App.Current.MainPage.DisplayAlert("Success", "User updated successfully", "OK");
                    }
                    else
                    {
                        ErrorMessage = "Failed to update user";
                    }
                }
                else
                {
                    ErrorMessage = "User not found";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving user: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            SelectedUser = null;

            // Clear form fields
            UserId = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            Role = "User";
            Points = 0;
            Coins = 0;
            CurrentStreak = 0;
            AvatarUrl = string.Empty;

            ErrorMessage = string.Empty;
        }

        private async Task DeleteUserAsync(UsersModel user)
        {
            if (user == null)
                return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete user '{user.FullName}'? This action cannot be undone.",
                "Yes", "No");

            if (!confirm)
                return;

            IsLoading = true;

            try
            {
                // Note: In a real application, you would need to implement user deletion in the Firebase Auth
                // For simplicity, we'll just show a message that it's not implemented
                await App.Current.MainPage.DisplayAlert(
                    "Not Implemented",
                    "User deletion is not implemented in this demo. In a real application, you would need to delete the user from Firebase Auth and then remove their data from the database.",
                    "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error deleting user: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewUserProgressAsync(UsersModel user)
        {
            if (user == null)
                return;

            // Navigate to user progress view
            await Shell.Current.GoToAsync($"userProgress?userId={user.UserId}");
        }

        private async Task ViewUserBadgesAsync(UsersModel user)
        {
            if (user == null)
                return;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var badges = await _badgeService.GetUserBadgesAsync(user.UserId, idToken);

                if (badges.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert(
                        "User Badges",
                        $"{user.FullName} has not earned any badges yet.",
                        "OK");
                    return;
                }

                // Format badge list for display
                var badgeList = string.Join("\n", badges.Select(b => $"• {b.Title} ({b.Tier})"));

                await App.Current.MainPage.DisplayAlert(
                    $"Badges for {user.FullName}",
                    $"Total badges: {badges.Count}\n\n{badgeList}",
                    "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error loading badges: {ex.Message}", "OK");
            }
        }

        private async Task ManageFriendsAsync(UsersModel user)
        {
            if (user == null)
                return;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var friends = await _userService.GetFriendsAsync(user.UserId, idToken);

                if (friends.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert(
                        "User Friends",
                        $"{user.FullName} has not added any friends yet.",
                        "OK");
                    return;
                }

                // Format friend list for display
                var friendList = string.Join("\n", friends.Select(f => $"• {f.FullName} ({f.Email})"));

                await App.Current.MainPage.DisplayAlert(
                    $"Friends of {user.FullName}",
                    $"Total friends: {friends.Count}\n\n{friendList}",
                    "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error loading friends: {ex.Message}", "OK");
            }
        }
    }
}
