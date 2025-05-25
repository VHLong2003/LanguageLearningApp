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

        // Các trường chỉnh sửa hồ sơ
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

        // Các thuộc tính chỉnh sửa hồ sơ
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

        // Các lệnh
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

                // Xác định người dùng cần tải
                string userIdToLoad = IsViewingOtherUser ? _viewedUserId : currentUserId;

                // Tải dữ liệu người dùng
                var user = await _userService.GetUserByIdAsync(userIdToLoad, idToken);

                if (user != null)
                {
                    User = user;

                    // Cập nhật các trường biểu mẫu nếu đây là người dùng hiện tại
                    if (!IsViewingOtherUser)
                    {
                        FullName = user.FullName;
                        Email = user.Email;
                        AvatarUrl = user.AvatarUrl;
                    }

                    // Tải huy hiệu
                    var userBadges = await _badgeService.GetUserBadgesAsync(userIdToLoad, idToken);

                    Badges.Clear();
                    foreach (var badge in userBadges)
                    {
                        Badges.Add(badge);
                    }

                    // Tải tiến trình gần đây
                    var allProgress = await _progressService.GetUserProgressAsync(userIdToLoad, idToken);

                    // Sắp xếp theo ngày hoàn thành (gần đây nhất trước) và lấy 5 mục đầu tiên
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
                    StatusMessage = "Không thể tải dữ liệu người dùng";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        private void StartEditProfile()
        {
            // Chỉ cho phép chỉnh sửa nếu đang xem hồ sơ của chính mình
            if (IsViewingOtherUser)
                return;

            IsEditing = true;

            // Xóa các trường mật khẩu
            OldPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        private async Task SaveProfileAsync()
        {
            // Chỉ cho phép lưu nếu đang xem hồ sơ của chính mình
            if (IsViewingOtherUser)
                return;

            // Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(FullName))
            {
                StatusMessage = "Vui lòng nhập họ tên đầy đủ";
                return;
            }

            // Kiểm tra mật khẩu nếu thay đổi
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (string.IsNullOrWhiteSpace(OldPassword))
                {
                    StatusMessage = "Vui lòng nhập mật khẩu hiện tại";
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    StatusMessage = "Mật khẩu mới không khớp";
                    return;
                }

                if (!ValidationHelper.IsValidPassword(NewPassword))
                {
                    StatusMessage = "Mật khẩu mới phải có ít nhất 6 ký tự";
                    return;
                }

                // TODO: Triển khai thay đổi mật khẩu qua Firebase Auth
            }

            IsLoading = true;
            StatusMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Cập nhật dữ liệu người dùng
                User.FullName = FullName;
                User.AvatarUrl = AvatarUrl;

                var success = await _userService.UpdateUserAsync(User, idToken);

                if (success)
                {
                    IsEditing = false;
                    StatusMessage = "Cập nhật hồ sơ thành công";
                }
                else
                {
                    StatusMessage = "Không thể cập nhật hồ sơ";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;

            // Đặt lại các trường biểu mẫu về giá trị hiện tại
            FullName = User.FullName;
            Email = User.Email;
            AvatarUrl = User.AvatarUrl;

            // Xóa các trường mật khẩu
            OldPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;

            StatusMessage = string.Empty;
        }

        private async Task ChangeAvatarAsync()
        {
            try
            {
                // Sử dụng bộ chọn tệp MAUI
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Chọn ảnh hồ sơ"
                });

                if (result != null)
                {
                    IsLoading = true;

                    var idToken = LocalStorageHelper.GetItem("idToken");
                    var userId = LocalStorageHelper.GetItem("userId");

                    // Tải ảnh lên
                    var imageUrl = await _storageService.UploadImageFromPickerAsync(result, "avatars", idToken);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Cập nhật URL ảnh đại diện
                        AvatarUrl = imageUrl;
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể tải ảnh lên", "OK");
                    }

                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi chọn ảnh: {ex.Message}", "OK");
            }
        }

        private async Task LogoutAsync()
        {
            var confirm = await App.Current.MainPage.DisplayAlert(
                "Xác nhận đăng xuất",
                "Bạn có chắc muốn đăng xuất không?",
                "Có", "Không");

            if (!confirm)
                return;

            IsLoading = true;

            try
            {
                var success = await _authService.LogoutAsync();

                if (success)
                {
                    // Chuyển đến trang đăng nhập
                    await Shell.Current.GoToAsync("//login");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi đăng xuất: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddFriendAsync()
        {
            // Chỉ cho phép thêm bạn khi đang xem hồ sơ của người khác
            if (!IsViewingOtherUser || string.IsNullOrEmpty(_viewedUserId))
                return;

            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var currentUserId = LocalStorageHelper.GetItem("userId");

                // Thêm bạn (quan hệ hai chiều)
                var addToCurrentUser = await _userService.AddFriendAsync(currentUserId, _viewedUserId, idToken);
                var addToOtherUser = await _userService.AddFriendAsync(_viewedUserId, currentUserId, idToken);

                if (addToCurrentUser && addToOtherUser)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Thêm bạn thành công", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể thêm bạn", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Lỗi khi thêm bạn: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}