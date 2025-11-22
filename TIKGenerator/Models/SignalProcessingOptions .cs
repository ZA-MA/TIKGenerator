using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TIKGenerator.Models
{
    public enum SignalProcessingType
    {
        None,
        LowPass,
        HighPass,
        BandPass,
        BandStop,
        MovingAverage,
        ExponentialSmoothing
    }

    public class SignalProcessingOptions : INotifyPropertyChanged
    {
        private SignalProcessingType _selectedProcessingType = SignalProcessingType.None;
        private double? _lowPassCutoff;
        private double? _highPassCutoff;
        private double? _bandPassLow;
        private double? _bandPassHigh;
        private double? _bandStopLow;
        private double? _bandStopHigh;
        private int? _movingAverageWindow;
        private double? _exponentialAlpha;
        private bool _useFFT;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ProcessingApplied;
        public event EventHandler ProcessingChanged;

        public SignalProcessingType SelectedProcessingType
        {
            get => _selectedProcessingType;
            set
            {
                if (_selectedProcessingType == value) return;
                _selectedProcessingType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNoneSelected));
                OnPropertyChanged(nameof(IsLowPassSelected));
                OnPropertyChanged(nameof(IsHighPassSelected));
                OnPropertyChanged(nameof(IsBandPassSelected));
                OnPropertyChanged(nameof(IsBandStopSelected));
                OnPropertyChanged(nameof(IsMovingAverageSelected));
                OnPropertyChanged(nameof(IsExponentialSmoothingSelected));
                OnProcessingChanged();
            }
        }
        public bool IsNoneSelected
        {
            get => _selectedProcessingType == SignalProcessingType.None;
            set { if (value) SelectedProcessingType = SignalProcessingType.None; OnProcessingChanged(); }
        }

        public bool IsLowPassSelected
        {
            get => _selectedProcessingType == SignalProcessingType.LowPass;
            set { if (value) SelectedProcessingType = SignalProcessingType.LowPass; OnProcessingChanged(); }
        }

        public bool IsHighPassSelected
        {
            get => _selectedProcessingType == SignalProcessingType.HighPass;
            set { if (value) SelectedProcessingType = SignalProcessingType.HighPass; OnProcessingChanged(); }
        }

        public bool IsBandPassSelected
        {
            get => _selectedProcessingType == SignalProcessingType.BandPass;
            set { if (value) SelectedProcessingType = SignalProcessingType.BandPass; OnProcessingChanged(); }
        }

        public bool IsBandStopSelected
        {
            get => _selectedProcessingType == SignalProcessingType.BandStop;
            set { if (value) SelectedProcessingType = SignalProcessingType.BandStop; OnProcessingChanged(); }
        }

        public bool IsMovingAverageSelected
        {
            get => _selectedProcessingType == SignalProcessingType.MovingAverage;
            set { if (value) SelectedProcessingType = SignalProcessingType.MovingAverage; OnProcessingChanged(); }
        }

        public bool IsExponentialSmoothingSelected
        {
            get => _selectedProcessingType == SignalProcessingType.ExponentialSmoothing;
            set { if (value) SelectedProcessingType = SignalProcessingType.ExponentialSmoothing; OnProcessingChanged(); }
        }

        public double? LowPassCutoff { get => _lowPassCutoff; set { _lowPassCutoff = value; OnPropertyChanged(); OnProcessingChanged(); } }
        public double? HighPassCutoff { get => _highPassCutoff; set { _highPassCutoff = value; OnPropertyChanged(); OnProcessingChanged(); } }
        public double? BandPassLow { get => _bandPassLow; set { _bandPassLow = value; OnPropertyChanged(); OnProcessingChanged(); } }
        public double? BandPassHigh { get => _bandPassHigh; set { _bandPassHigh = value; OnPropertyChanged(); OnProcessingChanged(); } }
        public double? BandStopLow { get => _bandStopLow; set { _bandStopLow = value; OnPropertyChanged(); OnProcessingChanged(); } }
        public double? BandStopHigh { get => _bandStopHigh; set { _bandStopHigh = value; OnPropertyChanged(); OnProcessingChanged(); } }
        public int? MovingAverageWindow { get => _movingAverageWindow; set { _movingAverageWindow = value; OnPropertyChanged(); OnProcessingChanged(); } }
        public double? ExponentialAlpha { get => _exponentialAlpha; set { _exponentialAlpha = value; OnPropertyChanged(); OnProcessingChanged(); } }

        public bool UseFFT
        {
            get => _useFFT;
            set { if (_useFFT == value) return; _useFFT = value; OnPropertyChanged(); OnProcessingChanged(); }
        }

        public bool IsAnyProcessingActive => _selectedProcessingType != SignalProcessingType.None;

        private ICommand _applyCommand;
        public ICommand ApplyCommand => _applyCommand ??= new RelayCommand(_ => ApplyProcessing());

        public void ApplyProcessing()
        {
            ProcessingApplied?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            SelectedProcessingType = SignalProcessingType.None;
            LowPassCutoff = null;
            HighPassCutoff = null;
            BandPassLow = null;
            BandPassHigh = null;
            BandStopLow = null;
            BandStopHigh = null;
            MovingAverageWindow = null;
            ExponentialAlpha = null;
            UseFFT = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual void OnProcessingChanged()
            => ProcessingChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);
    }
}
