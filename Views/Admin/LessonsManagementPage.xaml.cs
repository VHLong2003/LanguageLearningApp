using LanguageLearningApp.ViewModels.Admin;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Views.Admin
{
    public partial class LessonsManagementPage : ContentPage
    {
        public LessonsManagementPage()
        {
            InitializeComponent();

            // ViewModel đã được binding sẵn trong XAML
            // Nếu DI không tự inject thì có thể tạo bằng code như sau:
            // this.BindingContext = new AdminLessonViewModel(...);
        }

        // Xử lý sự kiện chọn bài học trong danh sách
        private void OnLessonSelected(object sender, SelectionChangedEventArgs e)
        {
            if (BindingContext is not AdminLessonViewModel vm)
                return;

            var selectedLesson = e.CurrentSelection?.FirstOrDefault() as LessonModel;

            // Nếu chọn đúng bài học thì cập nhật SelectedLesson trong ViewModel
            if (selectedLesson != null)
            {
                vm.SelectedLesson = selectedLesson;
            }

            // Bỏ chọn để UI đẹp (không highlight mãi)
            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }
        }
    }
}
