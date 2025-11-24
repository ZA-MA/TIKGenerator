using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TIKGenerator.Helpers;
using TIKGenerator.Models;

namespace TIKGenerator.ViewModels
{
    public class SignalProcessingViewModel : INotifyPropertyChanged
    {
        public SignalProcessingModel Processing { get; set; }
        public ObservableCollection<ProcessingMethod> ProcessingMethods { get; }
        public ICommand ApplyCommand { get; }
        public event Action<SignalProcessingModel> ProcessingApplied;

        public SignalProcessingViewModel()
        {
            Processing = new SignalProcessingModel();

            ProcessingMethods = new ObservableCollection<ProcessingMethod>
            {
                ProcessingMethod.None,
                ProcessingMethod.LowPass,
                ProcessingMethod.HighPass,
                ProcessingMethod.BandPass,
                ProcessingMethod.BandStop,
                ProcessingMethod.MovingAverage,
                ProcessingMethod.ExponentialSmoothing,
                ProcessingMethod.FFT,
                ProcessingMethod.Normalization,
                ProcessingMethod.Overlay
            };

            ApplyCommand = new RelayCommand(_ => ApplyProcessing());
        }

        private void ApplyProcessing()
        {
            ProcessingApplied?.Invoke(Processing);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
