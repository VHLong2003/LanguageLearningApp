using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace LanguageLearningApp.ViewModels.Admin
{
    public partial class AdminUserViewModel : ObservableObject
    {
        private readonly UserService _userService = new UserService();

        [ObservableProperty] private ObservableCollection<AppUser> users = new();
        [ObservableProperty] private string searchText;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string errorMessage;

        // Tải tất cả users luôn khi page khởi tạo
        public AdminUserViewModel()
        {
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        public async Task LoadUsersAsync()
        {
            IsBusy = true;
            ErrorMessage = "";
            try
            {
                var allUsers = await _userService.GetAllUsersAsync();
                Users = new ObservableCollection<AppUser>(allUsers.OrderByDescending(u => u.Points));
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SearchUsersAsync()
        {
            IsBusy = true;
            try
            {
                var allUsers = await _userService.GetAllUsersAsync();
                var filtered = allUsers.Where(u => string.IsNullOrEmpty(SearchText)
                    || (u.FullName?.ToLower().Contains(SearchText.ToLower()) ?? false)
                    || (u.Email?.ToLower().Contains(SearchText.ToLower()) ?? false)).ToList();
                Users = new ObservableCollection<AppUser>(filtered);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteUserAsync(AppUser user)
        {
            if (user == null) return;
            IsBusy = true;
            try
            {
                await _userService.DeleteUserAsync(user.UserId);
                Users.Remove(user);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
