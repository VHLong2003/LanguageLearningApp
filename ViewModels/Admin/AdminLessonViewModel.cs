using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace LanguageLearningApp.ViewModels.Admin
{
    public partial class AdminLessonViewModel : ObservableObject
    {
        private readonly LessonService _lessonService = new LessonService();

        [ObservableProperty] private ObservableCollection<Lesson> lessons = new();
        [ObservableProperty] private Lesson selectedLesson = new();
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private string courseId;

        public event EventHandler<Lesson> LessonSelected; // Sự kiện chuyển trang quản lý câu hỏi

        public AdminLessonViewModel(string courseId)
        {
            CourseId = courseId;
            _ = LoadLessonsAsync();
            SelectedLesson = new Lesson { CourseId = courseId };
        }

        [RelayCommand]
        public async Task LoadLessonsAsync()
        {
            IsBusy = true;
            ErrorMessage = "";
            try
            {
                var all = await _lessonService.GetLessonsByCourseIdAsync(CourseId);
                Lessons = new ObservableCollection<Lesson>(all.OrderBy(l => l.Order));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task AddOrUpdateLessonAsync()
        {
            if (SelectedLesson == null || string.IsNullOrWhiteSpace(SelectedLesson.Title))
            {
                ErrorMessage = "Nhập tiêu đề bài học!";
                return;
            }
            SelectedLesson.CourseId = CourseId;
            IsBusy = true;
            try
            {
                await _lessonService.AddOrUpdateLessonAsync(SelectedLesson);
                await LoadLessonsAsync();
                SelectedLesson = new Lesson { CourseId = CourseId };
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteLessonAsync(Lesson lesson)
        {
            if (lesson == null) return;
            IsBusy = true;
            try
            {
                await _lessonService.DeleteLessonAsync(lesson.LessonId);
                Lessons.Remove(lesson);
                if (SelectedLesson == lesson) SelectedLesson = new Lesson { CourseId = CourseId };
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void EditLesson(Lesson lesson)
        {
            if (lesson != null)
            {
                SelectedLesson = new Lesson
                {
                    LessonId = lesson.LessonId,
                    CourseId = lesson.CourseId,
                    Title = lesson.Title,
                    Content = lesson.Content,
                    Order = lesson.Order,
                    RequiredPointsToUnlock = lesson.RequiredPointsToUnlock
                };
            }
        }

        [RelayCommand]
        public void SelectLesson(Lesson lesson)
        {
            LessonSelected?.Invoke(this, lesson);
        }
    }
}
