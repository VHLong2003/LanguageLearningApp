using LanguageLearningApp.ViewModels.Admin;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Views.Admin
{
    public partial class CoursesManagementPage : ContentPage
    {
        private readonly AdminCourseViewModel _viewModel;

        public CoursesManagementPage(AdminCourseViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }

        // Xử lý khi chọn một khoá học trong danh sách
        private void OnCourseSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            var selectedCourse = e.CurrentSelection[0] as Models.CourseModel;
            if (selectedCourse != null)
            {
                _viewModel.SelectedCourse = selectedCourse;
            }

            // Bỏ chọn sau khi xử lý (giúp CollectionView không bị giữ trạng thái highlight)
            ((CollectionView)sender).SelectedItem = null;
        }

    }
}
