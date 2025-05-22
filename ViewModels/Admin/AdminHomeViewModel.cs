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
    public class AdminHomeViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly CourseService _courseService;
        private readonly LeaderboardService _leaderboardService;

        private UsersModel _adminUser;
        private int _totalUsers;
        private int _totalCourses;
        private int _totalActiveLessons;
        private int _newUsersToday;
        private ObservableCollection<UsersModel> _recentUsers;
        private ObservableCollection<CourseModel> _recentCourses;
        private bool _isRefreshing;

        public UsersModel AdminUser
        {
            get => _adminUser;
            set => SetProperty(ref _adminUser, value);
        }

        public int TotalUsers
        {
            get => _totalUsers;
            set => SetProperty(ref _totalUsers, value);
        }

        public int TotalCourses
        {
            get => _totalCourses;
            set => SetProperty(ref _totalCourses, value);
        }

        public int TotalActiveLessons
        {
            get => _totalActiveLessons;
            set => SetProperty(ref _totalActiveLessons, value);
        }

        public int NewUsersToday
        {
            get => _newUsersToday;
            set => SetProperty(ref _newUsersToday, value);
        }

        public ObservableCollection<UsersModel> RecentUsers
        {
            get => _recentUsers;
            set => SetProperty(ref _recentUsers, value);
        }

        public ObservableCollection<CourseModel> RecentCourses
        {
            get => _recentCourses;
            set => SetProperty(ref _recentCourses, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ManageUsersCommand { get; }
        public ICommand ManageCoursesCommand { get; }
        public ICommand ManageLessonsCommand { get; }
        public ICommand ManageShopCommand { get; }
        public ICommand UpdateLeaderboardCommand { get; }

        public AdminHomeViewModel(
            UserService userService,
            CourseService courseService,
            LeaderboardService leaderboardService)
        {
            _userService = userService;
            _courseService = courseService;
            _leaderboardService = leaderboardService;

            RecentUsers = new ObservableCollection<UsersModel>();
            RecentCourses = new ObservableCollection<CourseModel>();

            RefreshCommand = new Command(async () => await LoadDashboardDataAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            ManageUsersCommand = new Command(async () => await Shell.Current.GoToAsync("//users"));
            ManageCoursesCommand = new Command(async () => await Shell.Current.GoToAsync("//adminCourses"));
            ManageLessonsCommand = new Command(async () => await Shell.Current.GoToAsync("//lessons"));
            ManageShopCommand = new Command(async () => await Shell.Current.GoToAsync("//adminShop"));
            UpdateLeaderboardCommand = new Command(async () => await UpdateLeaderboardAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            IsRefreshing = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                // Load admin user data
                AdminUser = await _userService.GetUserByIdAsync(userId, idToken);

                // Load all users
                var allUsers = await _userService.GetAllUsersAsync(idToken);

                TotalUsers = allUsers.Count;

                // Count new users today
                var today = DateTime.Today;
                NewUsersToday = allUsers.FindAll(u => u.DateJoined.Date == today).Count;

                // Get recent users
                allUsers.Sort((a, b) => b.DateJoined.CompareTo(a.DateJoined));
                var recentUsers = allUsers.GetRange(0, Math.Min(5, allUsers.Count));

                RecentUsers.Clear();
                foreach (var user in recentUsers)
                {
                    RecentUsers.Add(user);
                }

                // Load courses
                var courses = await _courseService.GetAllCoursesAsync(idToken);

                TotalCourses = courses.Count;
                TotalActiveLessons = 0;

                foreach (var course in courses)
                {
                    // Count lessons for each course
                    TotalActiveLessons += course.TotalLessons;
                }

                // Get recent courses
                courses.Sort((a, b) => DateTime.Parse(b.CreatedDate).CompareTo(DateTime.Parse(a.CreatedDate)));
                var recentCourses = courses.GetRange(0, Math.Min(5, courses.Count));

                RecentCourses.Clear();
                foreach (var course in recentCourses)
                {
                    RecentCourses.Add(course);
                }

                // Update the leaderboard snapshot (for better app performance)
                await _leaderboardService.UpdateLeaderboardSnapshotAsync(idToken);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load dashboard: {ex.Message}", "OK");
            }
            finally
            {
                IsRefreshing = false;
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

            // Clear local storage
            LocalStorageHelper.Clear();

            // Navigate to login
            await Shell.Current.GoToAsync("///login");
        }

        private async Task UpdateLeaderboardAsync()
        {
            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Update leaderboard snapshot
                await _leaderboardService.UpdateLeaderboardSnapshotAsync(idToken);

                await App.Current.MainPage.DisplayAlert(
                    "Leaderboard Updated",
                    "The leaderboard has been updated successfully.",
                    "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Failed to update leaderboard: {ex.Message}",
                    "OK");
            }
        }
    }
}
