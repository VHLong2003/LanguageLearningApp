using System;
using LanguageLearningApp.ViewModels.User;

namespace LanguageLearningApp.Views.User
{
    [QueryProperty(nameof(LessonId), "lessonId")]
    [QueryProperty(nameof(CourseId), "courseId")]
    public partial class LessonPage : ContentPage
    {
        private LessonViewModel _viewModel;
        private string _lessonId;
        private string _courseId;

        public string LessonId
        {
            get => _lessonId;
            set
            {
                _lessonId = value;
                LoadLesson();
            }
        }

        public string CourseId
        {
            get => _courseId;
            set
            {
                _courseId = value;
                LoadLesson();
            }
        }

        public LessonPage(LessonViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadLesson();
        }

        private async void LoadLesson()
        {
            if (!string.IsNullOrEmpty(_lessonId) && !string.IsNullOrEmpty(_courseId))
            {
                await _viewModel.InitializeAsync(_lessonId, _courseId);
            }
        }
    }
}
