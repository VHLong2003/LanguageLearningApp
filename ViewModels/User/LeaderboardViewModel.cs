using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.User
{
    public class LeaderboardViewModel : BaseViewModel
    {
        private readonly LeaderboardService _leaderboardService;
        private readonly UserService _userService;

        private ObservableCollection<LeaderboardModel> _globalLeaderboard;
        private ObservableCollection<LeaderboardModel> _friendsLeaderboard;
        private ObservableCollection<LeaderboardModel> _weeklyLeaderboard;
        private bool _isRefreshing;
        private bool _showingFriends;
        private bool _showingWeekly;
        private LeaderboardModel _currentUserRank;
        private string _searchQuery;

        public ObservableCollection<LeaderboardModel> GlobalLeaderboard
        {
            get => _globalLeaderboard;
            set => SetProperty(ref _globalLeaderboard, value);
        }

        public ObservableCollection<LeaderboardModel> FriendsLeaderboard
        {
            get => _friendsLeaderboard;
            set => SetProperty(ref _friendsLeaderboard, value);
        }

        public ObservableCollection<LeaderboardModel> WeeklyLeaderboard
        {
            get => _weeklyLeaderboard;
            set => SetProperty(ref _weeklyLeaderboard, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool ShowingFriends
        {
            get => _showingFriends;
            set => SetProperty(ref _showingFriends, value);
        }

        public bool ShowingWeekly
        {
            get => _showingWeekly;
            set => SetProperty(ref _showingWeekly, value);
        }

        public LeaderboardModel CurrentUserRank
        {
            get => _currentUserRank;
            set => SetProperty(ref _currentUserRank, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    FilterLeaderboard();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ShowGlobalCommand { get; }
        public ICommand ShowFriendsCommand { get; }
        public ICommand ShowWeeklyCommand { get; }
        public ICommand ViewUserProfileCommand { get; }
        public ICommand SendFriendRequestCommand { get; }

        public LeaderboardViewModel(LeaderboardService leaderboardService, UserService userService)
        {
            _leaderboardService = leaderboardService;
            _userService = userService;

            GlobalLeaderboard = new ObservableCollection<LeaderboardModel>();
            FriendsLeaderboard = new ObservableCollection<LeaderboardModel>();
            WeeklyLeaderboard = new ObservableCollection<LeaderboardModel>();

            RefreshCommand = new Command(async () => await LoadLeaderboardAsync());
            ShowGlobalCommand = new Command(() => { ShowingFriends = false; ShowingWeekly = false; });
            ShowFriendsCommand = new Command(() => { ShowingFriends = true; ShowingWeekly = false; });
            ShowWeeklyCommand = new Command(() => { ShowingFriends = false; ShowingWeekly = true; });
            ViewUserProfileCommand = new Command<string>(async (userId) => await ViewUserProfileAsync(userId));
            SendFriendRequestCommand = new Command<string>(async (userId) => await SendFriendRequestAsync(userId));
        }

        public async Task InitializeAsync()
        {
            await LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            IsRefreshing = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Load global leaderboard
                var globalEntries = await _leaderboardService.GetGlobalLeaderboardAsync(idToken);

                GlobalLeaderboard.Clear();
                foreach (var entry in globalEntries)
                {
                    GlobalLeaderboard.Add(entry);

                    // If this is the current user, save their rank
                    if (entry.UserId == userId)
                    {
                        CurrentUserRank = entry;
                    }
                }

                // Load friends leaderboard
                var friendsEntries = await _leaderboardService.GetFriendsLeaderboardAsync(userId, idToken);

                FriendsLeaderboard.Clear();
                foreach (var entry in friendsEntries)
                {
                    FriendsLeaderboard.Add(entry);
                }

                // Load weekly leaderboard
                var weeklyEntries = await _leaderboardService.GetWeeklyLeaderboardAsync(idToken);

                WeeklyLeaderboard.Clear();
                foreach (var entry in weeklyEntries)
                {
                    WeeklyLeaderboard.Add(entry);
                }
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load leaderboard: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void FilterLeaderboard()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            var query = SearchQuery.ToLower();

            // Clone current data
            var originalGlobal = new List<LeaderboardModel>(GlobalLeaderboard);
            var originalFriends = new List<LeaderboardModel>(FriendsLeaderboard);
            var originalWeekly = new List<LeaderboardModel>(WeeklyLeaderboard);

            // Filter by username
            var filteredGlobal = originalGlobal.Where(e => e.UserName.ToLower().Contains(query)).ToList();
            var filteredFriends = originalFriends.Where(e => e.UserName.ToLower().Contains(query)).ToList();
            var filteredWeekly = originalWeekly.Where(e => e.UserName.ToLower().Contains(query)).ToList();

            // Update UI
            GlobalLeaderboard.Clear();
            FriendsLeaderboard.Clear();
            WeeklyLeaderboard.Clear();

            foreach (var entry in filteredGlobal)
                GlobalLeaderboard.Add(entry);

            foreach (var entry in filteredFriends)
                FriendsLeaderboard.Add(entry);

            foreach (var entry in filteredWeekly)
                WeeklyLeaderboard.Add(entry);
        }

        public async Task ViewUserProfileAsync(string userId)
        {
            // Navigate to user profile view
            await Shell.Current.GoToAsync($"userProfile?userId={userId}");
        }

        private async Task SendFriendRequestAsync(string userId)
        {
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var currentUserId = LocalStorageHelper.GetItem("userId");

                if (userId == currentUserId)
                {
                    await App.Current.MainPage.DisplayAlert("Friend Request", "You cannot add yourself as a friend", "OK");
                    return;
                }

                // Check if already friends
                var currentUser = await _userService.GetUserByIdAsync(currentUserId, idToken);
                if (currentUser.FriendIds != null && currentUser.FriendIds.Contains(userId))
                {
                    await App.Current.MainPage.DisplayAlert("Friend Request", "You are already friends with this user", "OK");
                    return;
                }

                // Send friend request (in this simple implementation, we directly add as friend)
                var success = await _userService.AddFriendAsync(currentUserId, userId, idToken);

                if (success)
                {
                    // Also add the current user to the other user's friend list (bidirectional friendship)
                    await _userService.AddFriendAsync(userId, currentUserId, idToken);

                    await App.Current.MainPage.DisplayAlert("Friend Request", "Friend added successfully!", "OK");

                    // Refresh leaderboard to show new friend
                    await LoadLeaderboardAsync();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Friend Request", "Failed to add friend", "OK");
                }
            }
            catch (System.Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error sending friend request: {ex.Message}", "OK");
            }
        }
    }
}
