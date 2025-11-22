using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace TIKGenerator.Views.Components.ProgressBar
{
    public partial class ProgressBar : UserControl, INotifyPropertyChanged
    {
        private bool _isVisible;
        private string _status = "Генерация...";
        private double _progress;
        private bool _isIndeterminate;

        private ICommand _cancelCommand;

        public ProgressBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set { _isIndeterminate = value; OnPropertyChanged(nameof(IsIndeterminate)); }
        }

        public ICommand CancelCommand
        {
            get => _cancelCommand;
            set { _cancelCommand = value; OnPropertyChanged(nameof(CancelCommand)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
