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
    public class AdminCourseViewModel : BaseViewModel
    {
        private readonly CourseService _courseService;
        private readonly StorageService _storageService;

        private ObservableCollection<CourseModel> _courses;
        private CourseModel _selectedCourse;
        private bool _isNewCourse;
        private bool _isEditing;
        private bool _isRefreshing;
        private bool _isBusy;
        private string _errorMessage;

        // Form fields
        private string _title;
        private string _description;
        private CourseType _type;
        private int _difficultyLevel;
        private int _requiredPoints;
        private string _imageUrl;
        private bool _isPublished;

        public ObservableCollection<CourseModel> Courses
        {
            get => _courses;
            set => SetProperty(ref _courses, value);
        }

        public CourseModel SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (SetProperty(ref _selectedCourse, value) && value != null)
                {
                    // Update form fields
                    Title = value.Title;
                    Description = value.Description;
                    Type = value.Type;
                    DifficultyLevel = value.DifficultyLevel;
                    RequiredPoints = value.RequiredPointsToUnlock;
                    ImageUrl = value.ImageUrl;
                    IsPublished = value.IsPublished;

                    IsEditing = true;
                    IsNewCourse = false;
                }
            }
        }

        public bool IsNewCourse
        {
            get => _isNewCourse;
            set => SetProperty(ref _isNewCourse, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Form properties
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

        public CourseType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public int DifficultyLevel
        {
            get => _difficultyLevel;
            set => SetProperty(ref _difficultyLevel, value);
        }

        public int RequiredPoints
        {
            get => _requiredPoints;
            set => SetProperty(ref _requiredPoints, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public bool IsPublished
        {
            get => _isPublished;
            set => SetProperty(ref _isPublished, value);
        }

        // Course type options for dropdown
        public Array CourseTypes => Enum.GetValues(typeof(CourseType));

        // Commands
        public ICommand LoadCoursesCommand { get; }
        public ICommand NewCourseCommand { get; }
        public ICommand SaveCourseCommand { get; }
        public ICommand DeleteCourseCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand ManageLessonsCommand { get; }

        public AdminCourseViewModel(CourseService courseService, StorageService storageService)
        {
            _courseService = courseService;
            _storageService = storageService;

            Courses = new ObservableCollection<CourseModel>();

            LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
            NewCourseCommand = new Command(CreateNewCourse);
            SaveCourseCommand = new Command(async () => await SaveCourseAsync());
            DeleteCourseCommand = new Command<CourseModel>(async (course) => await DeleteCourseAsync(course));
            CancelEditCommand = new Command(CancelEdit);
            PickImageCommand = new Command(async () => await PickImageAsync());
            ManageLessonsCommand = new Command<CourseModel>(async (course) => await ManageLessonsAsync(course));

            // Default values for new course
            DifficultyLevel = 1;
            RequiredPoints = 0;
        }

        public async Task InitializeAsync()
        {
            await LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            IsRefreshing = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var allCourses = await _courseService.GetAllCoursesAsync(idToken);

                Courses.Clear();
                foreach (var course in allCourses)
                {
                    Courses.Add(course);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading courses: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void CreateNewCourse()
        {
            // Clear form fields
            Title = string.Empty;
            Description = string.Empty;
            Type = CourseType.Language;
            DifficultyLevel = 1;
            RequiredPoints = 0;
            ImageUrl = string.Empty;
            IsPublished = false;

            IsNewCourse = true;
            IsEditing = true;
            SelectedCourse = null;
            ErrorMessage = string.Empty;
        }

        private async Task SaveCourseAsync()
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

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var userId = LocalStorageHelper.GetItem("userId");

                if (IsNewCourse)
                {
                    // Create new course
                    var newCourse = new CourseModel
                    {
                        Title = Title,
                        Description = Description,
                        Type = Type,
                        DifficultyLevel = DifficultyLevel,
                        RequiredPointsToUnlock = RequiredPoints,
                        ImageUrl = ImageUrl,
                        IsPublished = IsPublished,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now.ToString("o"),
                        TotalLessons = 0
                    };

                    var courseId = await _courseService.CreateCourseAsync(newCourse, idToken);

                    if (courseId != null)
                    {
                        newCourse.CourseId = courseId;
                        Courses.Add(newCourse);

                        // Reset form
                        CancelEdit();
                    }
                    else
                    {
                        ErrorMessage = "Failed to create course";
                    }
                }
                else
                {
                    // Update existing course
                    if (SelectedCourse != null)
                    {
                        SelectedCourse.Title = Title;
                        SelectedCourse.Description = Description;
                        SelectedCourse.Type = Type;
                        SelectedCourse.DifficultyLevel = DifficultyLevel;
                        SelectedCourse.RequiredPointsToUnlock = RequiredPoints;
                        SelectedCourse.ImageUrl = ImageUrl;
                        SelectedCourse.IsPublished = IsPublished;

                        var success = await _courseService.UpdateCourseAsync(SelectedCourse, idToken);

                        if (success)
                        {
                            // Reset form
                            CancelEdit();
                        }
                        else
                        {
                            ErrorMessage = "Failed to update course";
                        }
                    }
                }

                // Refresh the list
                await LoadCoursesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving course: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteCourseAsync(CourseModel course)
        {
            if (course == null) return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete '{course.Title}'? This will also delete all associated lessons and questions.",
                "Yes", "No");

            if (!confirm) return;

            IsBusy = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var success = await _courseService.DeleteCourseAsync(course.CourseId, idToken);

                if (success)
                {
                    Courses.Remove(course);

                    // If the deleted course was being edited, clear the form
                    if (SelectedCourse == course)
                    {
                        CancelEdit();
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to delete course", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error deleting course: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            IsNewCourse = false;
            SelectedCourse = null;
            ErrorMessage = string.Empty;

            // Clear form fields
            Title = string.Empty;
            Description = string.Empty;
            Type = CourseType.Language;
            DifficultyLevel = 1;
            RequiredPoints = 0;
            ImageUrl = string.Empty;
            IsPublished = false;
        }

        private async Task PickImageAsync()
        {
            try
            {
                // Use the MAUI file picker
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select Course Image"
                });

                if (result != null)
                {
                    // Upload the image
                    IsBusy = true;
                    var idToken = LocalStorageHelper.GetItem("idToken");
                    var imageUrl = await _storageService.UploadImageFromPickerAsync(result, "course_images", idToken);

                    if (imageUrl != null)
                    {
                        ImageUrl = imageUrl;
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Failed to upload image", "OK");
                    }

                    IsBusy = false;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error picking image: {ex.Message}", "OK");
            }
        }

        private async Task ManageLessonsAsync(CourseModel course)
        {
            if (course == null) return;

            // Navigate to the lessons management page for this course
            await Shell.Current.GoToAsync($"lessons?courseId={course.CourseId}");
        }
    }
}
