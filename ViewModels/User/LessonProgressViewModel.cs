﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using LanguageLearningApp.Services;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.ViewModels.User
{
    public class LessonProgressViewModel : BaseViewModel
    {
        private readonly LessonProgressService _lessonProgressService;
        private readonly CourseService _courseService;
        private readonly UserService _userService;

        private string _userId;
        private UsersModel _user;
        private ObservableCollection<CourseProgressItem> _courses;
        private double _overallProgress;
        private int _totalPoints;
        private int _totalCompletedLessons;
        private bool _isLoading;
        private bool _isRefreshing;

        public UsersModel User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public ObservableCollection<CourseProgressItem> Courses
        {
            get => _courses;
            set => SetProperty(ref _courses, value);
        }

        public double OverallProgress
        {
            get => _overallProgress;
            set => SetProperty(ref _overallProgress, value);
        }

        public int TotalPoints
        {
            get => _totalPoints;
            set => SetProperty(ref _totalPoints, value);
        }

        public int TotalCompletedLessons
        {
            get => _totalCompletedLessons;
            set => SetProperty(ref _totalCompletedLessons, value);
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

        public ICommand RefreshCommand { get; }
        public ICommand ViewCourseCommand { get; }

        public LessonProgressViewModel(
            LessonProgressService lessonProgressService,
            CourseService courseService,
            UserService userService)
        {
            _lessonProgressService = lessonProgressService;
            _courseService = courseService;
            _userService = userService;

            Courses = new ObservableCollection<CourseProgressItem>();

            RefreshCommand = new Command(async () => await LoadProgressAsync());
            ViewCourseCommand = new Command<string>(async (courseId) => await ViewCourseAsync(courseId));
        }

        public async Task InitializeAsync()
        {
            _userId = LocalStorageHelper.GetItem("userId");
            await LoadProgressAsync();
        }

        private async Task LoadProgressAsync()
        {
            IsLoading = true;
            IsRefreshing = true;

            try
            {
                var idToken = LocalStorageHelper.GetItem("idToken");

                // Tải dữ liệu người dùng
                User = await _userService.GetUserByIdAsync(_userId, idToken);

                // Tải tất cả khóa học
                var allCourses = await _courseService.GetAllCoursesAsync(idToken);

                // Đặt lại các bộ đếm
                TotalPoints = User.Points;
                TotalCompletedLessons = 0;
                int totalCoursesStarted = 0;

                Courses.Clear();

                foreach (var course in allCourses)
                {
                    // Lấy tiến trình cho khóa học này
                    var progress = await _lessonProgressService.GetOverallCourseProgressAsync(_userId, course.CourseId, idToken);

                    if (progress > 0)
                    {
                        totalCoursesStarted++;

                        // Tính số bài học đã hoàn thành
                        int completedLessons = (int)Math.Round(progress * course.TotalLessons / 100);
                        TotalCompletedLessons += completedLessons;

                        // Thêm vào bộ sưu tập khóa học
                        Courses.Add(new CourseProgressItem
                        {
                            CourseId = course.CourseId,
                            Title = course.Title,
                            ImageUrl = course.ImageUrl,
                            Type = course.Type,
                            TotalLessons = course.TotalLessons,
                            CompletedLessons = completedLessons,
                            Progress = progress / 100
                        });
                    }
                }

                // Tính tiến trình tổng thể (trung bình của các khóa học đã bắt đầu)
                if (totalCoursesStarted > 0)
                {
                    double totalProgress = 0;
                    foreach (var course in Courses)
                    {
                        totalProgress += course.Progress;
                    }
                    OverallProgress = totalProgress / totalCoursesStarted;
                }
                else
                {
                    OverallProgress = 0;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải tiến trình: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        private async Task ViewCourseAsync(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
                return;

            // Chuyển đến chi tiết khóa học
            await Shell.Current.GoToAsync($"courseDetail?courseId={courseId}");
        }
    }

    public class CourseProgressItem
    {
        public string CourseId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public CourseType Type { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public double Progress { get; set; }
    }
}