using System.ComponentModel;

namespace LanguageLearningApp.Models
{
    public class DragPair : INotifyPropertyChanged
    {
        private string left;
        private string right;

        public string Left
        {
            get => left;
            set
            {
                if (left != value)
                {
                    left = value;
                    OnPropertyChanged(nameof(Left));
                }
            }
        }

        public string Right
        {
            get => right;
            set
            {
                if (right != value)
                {
                    right = value;
                    OnPropertyChanged(nameof(Right));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
