using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TIKGenerator.Models
{
    public enum ProcessingMethod
    {
        None,
        LowPass,
        HighPass,
        BandPass,
        BandStop,
        MovingAverage,
        ExponentialSmoothing,
        FFT,
        Normalization,
        Overlay
    }

    public class SignalProcessingModel : INotifyPropertyChanged
    {
        private ProcessingMethod _selectedMethod = ProcessingMethod.None;
        public ProcessingMethod SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                if (_selectedMethod != value)
                {
                    _selectedMethod = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _lowPassCutoff;
        public double? LowPassCutoff
        {
            get => _lowPassCutoff;
            set
            {
                if (_lowPassCutoff != value)
                {
                    _lowPassCutoff = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _highPassCutoff;
        public double? HighPassCutoff
        {
            get => _highPassCutoff;
            set
            {
                if (_highPassCutoff != value)
                {
                    _highPassCutoff = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _bandPassLow;
        public double? BandPassLow
        {
            get => _bandPassLow;
            set
            {
                if (_bandPassLow != value)
                {
                    _bandPassLow = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _bandPassHigh;
        public double? BandPassHigh
        {
            get => _bandPassHigh;
            set
            {
                if (_bandPassHigh != value)
                {
                    _bandPassHigh = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _bandStopLow;
        public double? BandStopLow
        {
            get => _bandStopLow;
            set
            {
                if (_bandStopLow != value)
                {
                    _bandStopLow = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _bandStopHigh;
        public double? BandStopHigh
        {
            get => _bandStopHigh;
            set
            {
                if (_bandStopHigh != value)
                {
                    _bandStopHigh = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _movingAverageWindow;
        public int? MovingAverageWindow
        {
            get => _movingAverageWindow;
            set
            {
                if (_movingAverageWindow != value)
                {
                    _movingAverageWindow = value;
                    OnPropertyChanged();
                }
            }
        }

        private double? _exponentialAlpha;
        public double? ExponentialAlpha
        {
            get => _exponentialAlpha;
            set
            {
                if (_exponentialAlpha != value)
                {
                    _exponentialAlpha = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}