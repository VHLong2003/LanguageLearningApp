using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace LanguageLearningApp.ViewModels.User
{
    public partial class UserAccountViewModel : ObservableObject
    {
        private readonly UserService _userService = new UserService();

        [ObservableProperty] private AppUser currentUser;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string errorMessage;

        public ICommand LogoutCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand ChangeAvatarCommand { get; }

        public UserAccountViewModel()
        {
            LogoutCommand = new RelayCommand(Logout);
            EditProfileCommand = new AsyncRelayCommand(EditProfileAsync);
            ChangeAvatarCommand = new AsyncRelayCommand(ChangeAvatarAsync);
            _ = LoadUserAsync();
        }

        public async Task LoadUserAsync()
        {
            IsBusy = true;
            ErrorMessage = "";
            try
            {
                var userId = AuthStorage.GetUserId();
                var idToken = AuthStorage.GetIdToken();
                var user = await _userService.GetUserByIdAsync(userId, idToken);
                CurrentUser = user;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Không thể tải thông tin tài khoản. " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Logout()
        {
            AuthStorage.ClearLogin();
            Application.Current.MainPage = new NavigationPage(new Views.User.LoginPage());
        }

        private async Task EditProfileAsync()
        {
            if (CurrentUser == null)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy thông tin tài khoản.", "OK");
                return;
            }

            var newName = await Shell.Current.DisplayPromptAsync("Đổi tên", "Nhập tên mới:", initialValue: CurrentUser.FullName);
            if (!string.IsNullOrWhiteSpace(newName) && newName != CurrentUser.FullName)
            {
                IsBusy = true;
                try
                {
                    CurrentUser.FullName = newName;
                    var idToken = AuthStorage.GetIdToken();
                    await _userService.SaveUserAsync(CurrentUser, idToken);
                }
                catch (System.Exception ex)
                {
                    await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }


        private async Task ChangeAvatarAsync()
        {
            try
            {
                FileResult photo = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Chọn ảnh đại diện",
                    FileTypes = FilePickerFileType.Images
                });

                if (photo != null)
                {
                    // Giả sử bạn upload lên Firebase Storage ở đây rồi lấy URL về
                    // Ở đây mình demo lấy local file
                    var localPath = photo.FullPath;
                    CurrentUser.AvatarUrl = localPath;
                    var idToken = AuthStorage.GetIdToken();
                    await _userService.SaveUserAsync(CurrentUser, idToken);
                    OnPropertyChanged(nameof(CurrentUser));
                }
            }
            catch (System.Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }
    }
}
