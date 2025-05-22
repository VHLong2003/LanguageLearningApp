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
    [QueryProperty(nameof(CourseId), "courseId")]
    public class AdminLessonViewModel : BaseViewModel
    {
        private readonly LessonService _lessonService;
        private readonly CourseService _courseService;
        private readonly StorageService _storageService;

        private string _courseId;
        private CourseModel _currentCourse;
        private ObservableCollection<LessonModel> _lessons;
        private LessonModel _selectedLesson;
        private bool _isRefreshing;
        private bool _isLoading;
        private bool _isEditing;
        private bool _isNewLesson;
        private string _errorMessage;

        // Form fields for editing/creating lesson
        private string _lessonId;
        private string _title;
        private string _description;
        private string _content;
        private string _imageUrl;
        private string _videoUrl;
        private int _order;
        private int _requiredPoints;
        private int _maxPoints;
        private int _estimatedMinutes;

        public string CourseId
        {
            get => _courseId;
            set
            {
                if (SetProperty(ref _courseId, value) && !string.IsNullOrEmpty(value))
                {
                    // When courseId is set via QueryProperty, load data
                    Task.Run(async () => await LoadCourseAndLessonsAsync());
                }
            }
        }

        public CourseModel CurrentCourse
        {
            get => _currentCourse;
            set => SetProperty(ref _currentCourse, value);
        }

        public ObservableCollection<LessonModel> Lessons
        {
            get => _lessons;
            set => SetProperty(ref _lessons, value);
        }

        public LessonModel SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                if (SetProperty(ref _selectedLesson, value) && value != null)
                {
                    // Update form fields with selected lesson
                    LessonId = value.LessonId;
                    Title = value.Title;
                    Description = value.Description;
                    Content = value.Content;
                    ImageUrl = value.ImageUrl;
                    VideoUrl = value.VideoUrl;
                    Order = value.Order;
                    RequiredPoints = value.RequiredPointsToUnlock;
                    MaxPoints = value.MaxPoints;
                    EstimatedMinutes = value.EstimatedMinutes;

                    IsEditing = true;
                    IsNewLesson = false;
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

        public bool IsNewLesson
        {
            get => _isNewLesson;
            set => SetProperty(ref _isNewLesson, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Form properties
        public string LessonId
        {
            get => _lessonId;
            set => SetProperty(ref _lessonId, value);
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

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public string VideoUrl
        {
            get => _videoUrl;
            set => SetProperty(ref _videoUrl, value);
        }

        public int Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public int RequiredPoints
        {
            get => _requiredPoints;
            set => SetProperty(ref _requiredPoints, value);
        }

        public int MaxPoints
        {
            get => _maxPoints;
            set => SetProperty(ref _maxPoints, value);
        }

        public int EstimatedMinutes
        {
            get => _estimatedMinutes;
            set => SetProperty(ref _estimatedMinutes, value);
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand CreateLessonCommand { get; }
        public ICommand SaveLessonCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteLessonCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand ManageQuestionsCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand BackToCoursesCommand { get; }

        public AdminLessonViewModel(
            LessonService lessonService,
            CourseService courseService,
            StorageService storageService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
            _storageService = storageService;

            Lessons = new ObservableCollection<LessonModel>();

            RefreshCommand = new Command(async () => await LoadLessonsAsync());
            CreateLessonCommand = new Command(CreateNewLesson);
            SaveLessonCommand = new Command(async () => await SaveLessonAsync());
            CancelEditCommand = new Command(CancelEdit);
            DeleteLessonCommand = new Command<LessonModel>(async (lesson) => await DeleteLessonAsync(lesson));
            PickImageCommand = new Command(async () => await PickImageAsync());
            ManageQuestionsCommand = new Command<LessonModel>(async (lesson) => await ManageQuestionsAsync(lesson));
            MoveUpCommand = new Command<LessonModel>(async (lesson) => await MoveUpAsync(lesson));
            MoveDownCommand = new Command<LessonModel>(async (lesson) => await MoveDownAsync(lesson));
            BackToCoursesCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task LoadCourseAndLessonsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Load course details
                CurrentCourse = await _courseService.GetCourseByIdAsync(CourseId, idToken);

                if (CurrentCourse != null)
                {
                    await LoadLessonsAsync();
                }
                else
                {
                    ErrorMessage = "Course not found";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading course: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadLessonsAsync()
        {
            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var courseLessons = await _lessonService.GetLessonsByCourseIdAsync(CourseId, idToken);

                // Sort lessons by order
                courseLessons.Sort((a, b) => a.Order.CompareTo(b.Order));

                Lessons.Clear();
                foreach (var lesson in courseLessons)
                {
                    Lessons.Add(lesson);
                }

                // Update lesson count in course
                if (CurrentCourse != null && CurrentCourse.TotalLessons != courseLessons.Count)
                {
                    CurrentCourse.TotalLessons = courseLessons.Count;
                    await _courseService.UpdateCourseAsync(CurrentCourse, idToken);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading lessons: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void CreateNewLesson()
        {
            // Clear form fields
            LessonId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Content = string.Empty;
            ImageUrl = string.Empty;
            VideoUrl = string.Empty;
            Order = Lessons.Count + 1; // Set order to next in sequence
            RequiredPoints = 0;
            MaxPoints = 100;
            EstimatedMinutes = 10;

            IsNewLesson = true;
            IsEditing = true;
            SelectedLesson = null;
            ErrorMessage = string.Empty;
        }

        private async Task SaveLessonAsync()
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

            if (string.IsNullOrWhiteSpace(Content))
            {
                ErrorMessage = "Content is required";
                return;
            }

            if (Order <= 0)
            {
                ErrorMessage = "Order must be greater than 0";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                if (IsNewLesson)
                {
                    // Create new lesson
                    var newLesson = new LessonModel
                    {
                        CourseId = CourseId,
                        Title = Title,
                        Description = Description,
                        Content = Content,
                        ImageUrl = ImageUrl,
                        VideoUrl = VideoUrl,
                        Order = Order,
                        RequiredPointsToUnlock = RequiredPoints,
                        MaxPoints = MaxPoints,
                        EstimatedMinutes = EstimatedMinutes
                    };

                    var lessonId = await _lessonService.CreateLessonAsync(newLesson, idToken);

                    if (!string.IsNullOrEmpty(lessonId))
                    {
                        newLesson.LessonId = lessonId;

                        // Update course total lessons
                        if (CurrentCourse != null)
                        {
                            CurrentCourse.TotalLessons++;
                            await _courseService.UpdateCourseAsync(CurrentCourse, idToken);
                        }

                        // Reset form and refresh lessons
                        CancelEdit();
                        await LoadLessonsAsync();
                    }
                    else
                    {
                        ErrorMessage = "Failed to create lesson";
                    }
                }
                else
                {
                    // Update existing lesson
                    if (_selectedLesson != null)
                    {
                        _selectedLesson.Title = Title;
                        _selectedLesson.Description = Description;
                        _selectedLesson.Content = Content;
                        _selectedLesson.ImageUrl = ImageUrl;
                        _selectedLesson.VideoUrl = VideoUrl;
                        _selectedLesson.Order = Order;
                        _selectedLesson.RequiredPointsToUnlock = RequiredPoints;
                        _selectedLesson.MaxPoints = MaxPoints;
                        _selectedLesson.EstimatedMinutes = EstimatedMinutes;

                        var success = await _lessonService.UpdateLessonAsync(_selectedLesson, idToken);

                        if (success)
                        {
                            // Reset form and refresh lessons
                            CancelEdit();
                            await LoadLessonsAsync();
                        }
                        else
                        {
                            ErrorMessage = "Failed to update lesson";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving lesson: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            IsNewLesson = false;
            SelectedLesson = null;

            // Clear form fields
            LessonId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Content = string.Empty;
            ImageUrl = string.Empty;
            VideoUrl = string.Empty;
            Order = 0;
            RequiredPoints = 0;
            MaxPoints = 0;
            EstimatedMinutes = 0;

            ErrorMessage = string.Empty;
        }

        private async Task DeleteLessonAsync(LessonModel lesson)
        {
            if (lesson == null)
                return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete the lesson '{lesson.Title}'? This will also delete all associated questions.",
                "Yes", "No");

            if (!confirm)
                return;

            IsLoading = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");
                var success = await _lessonService.DeleteLessonAsync(lesson.LessonId, idToken);

                if (success)
                {
                    Lessons.Remove(lesson);

                    // Update course total lessons
                    if (CurrentCourse != null)
                    {
                        CurrentCourse.TotalLessons--;
                        await _courseService.UpdateCourseAsync(CurrentCourse, idToken);
                    }

                    // Reorder remaining lessons
                    await ReorderLessonsAsync();

                    // If the deleted lesson was being edited, clear the form
                    if (SelectedLesson == lesson)
                    {
                        CancelEdit();
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to delete lesson", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Error deleting lesson: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PickImageAsync()
        {
            try
            {
                // Use MAUI file picker
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select Lesson Image"
                });

                if (result != null)
                {
                    IsLoading = true;

                    var idToken = LocalStorageHelper.GetItem("idToken");
                    var imageUrl = await _storageService.UploadImageFromPickerAsync(result, "lesson_images", idToken);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        ImageUrl = imageUrl;
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
                await App.Current.MainPage.DisplayAlert("Error", $"Error picking image: {ex.Message}", "OK");
            }
        }

        private async Task ManageQuestionsAsync(LessonModel lesson)
        {
            if (lesson == null)
                return;

            // Navigate to questions management for this lesson
            await Shell.Current.GoToAsync($"questionManagement?lessonId={lesson.LessonId}&courseId={CourseId}");
        }

        private async Task MoveUpAsync(LessonModel lesson)
        {
            if (lesson == null)
                return;

            var index = Lessons.IndexOf(lesson);

            // Can't move up if already first
            if (index <= 0)
                return;

            // Swap with previous lesson
            var previousLesson = Lessons[index - 1];

            // Swap order values
            int tempOrder = lesson.Order;
            lesson.Order = previousLesson.Order;
            previousLesson.Order = tempOrder;

            // Update lessons in database
            var idToken = LocalStorageHelper.GetItem("idToken");
            await _lessonService.UpdateLessonAsync(lesson, idToken);
            await _lessonService.UpdateLessonAsync(previousLesson, idToken);

            // Reload lessons to refresh order
            await LoadLessonsAsync();
        }

        private async Task MoveDownAsync(LessonModel lesson)
        {
            if (lesson == null)
                return;

            var index = Lessons.IndexOf(lesson);

            // Can't move down if already last
            if (index >= Lessons.Count - 1)
                return;

            // Swap with next lesson
            var nextLesson = Lessons[index + 1];

            // Swap order values
            int tempOrder = lesson.Order;
            lesson.Order = nextLesson.Order;
            nextLesson.Order = tempOrder;

            // Update lessons in database
            var idToken = LocalStorageHelper.GetItem("idToken");
            await _lessonService.UpdateLessonAsync(lesson, idToken);
            await _lessonService.UpdateLessonAsync(nextLesson, idToken);

            // Reload lessons to refresh order
            await LoadLessonsAsync();
        }

        private async Task ReorderLessonsAsync()
        {
            // Reorder remaining lessons
            var idToken = LocalStorageHelper.GetItem("idToken");

            for (int i = 0; i < Lessons.Count; i++)
            {
                var lesson = Lessons[i];
                lesson.Order = i + 1;
                await _lessonService.UpdateLessonAsync(lesson, idToken);
            }
        }
    }
}
