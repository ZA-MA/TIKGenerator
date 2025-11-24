using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TIKGenerator.Models;
using TIKGenerator.Services;

namespace TIKGenerator.ViewModels
{
    public class GeneratorViewModel : INotifyPropertyChanged
    {
        private readonly ISignalGeneratorService _generator;
        private readonly ISignalProcessingService _processor;
        private readonly ISignalGroupService _groupService;

        private readonly List<double[]> _rawSignals = new();

        private readonly List<ObservableCollection<ObservablePoint>> _seriesPoints = new();
        public ObservableCollection<ISeries> Series { get; } = new();

        private SignalGroup _signalGroup = new();
        public SignalGroup SignalGroup
        {
            get => _signalGroup;
            set
            {
                _signalGroup = value;
                OnPropertyChanged(nameof(SignalGroup));
            }
        }

        public Axis[] XAxes { get; }
        public Axis[] YAxes { get; }
        public SignalProcessingModel Processing { get; }

        private double _globalMinTime = 0;
        private double _globalMaxTime = 10;
        private const double MAX_TIME_RANGE = 1000;
        private const double MAX_VIEW_RANGE = 100;

        public double GlobalMinTime => _globalMinTime;
        public double GlobalMaxTime => _globalMaxTime;

        private double _viewStart;
        public double ViewStart
        {
            get => _viewStart;
            set
            {
                if (value > ViewEnd) value = ViewEnd;
                if (_viewStart == value) return;
                _viewStart = value;
                OnPropertyChanged(nameof(ViewStart));
                UpdateViewRange();
                UpdateStatistics();
            }
        }

        private double _viewEnd;
        public double ViewEnd
        {
            get => _viewEnd;
            set
            {
                if (value < ViewStart) value = ViewStart;
                if (_viewEnd == value) return;
                _viewEnd = value;
                OnPropertyChanged(nameof(ViewEnd));
                UpdateViewRange();
                UpdateStatistics();
            }
        }

        private double _generationProgress;
        public double GenerationProgress
        {
            get => _generationProgress;
            set
            {
                _generationProgress = value;
                OnPropertyChanged(nameof(GenerationProgress));
            }
        }

        private double _mean;
        public double Mean { get => _mean; set { _mean = Math.Round(value, 3); OnPropertyChanged(nameof(Mean)); } }

        private double _max;
        public double Max { get => _max; set { _max = Math.Round(value, 3); OnPropertyChanged(nameof(Max)); } }

        private double _min;
        public double Min { get => _min; set { _min = Math.Round(value, 3); OnPropertyChanged(nameof(Min)); } }


        public GeneratorViewModel(
            ISignalGeneratorService generator,
            ISignalProcessingService processor,
            ISignalGroupService groupService)
        {
            _generator = generator;
            _processor = processor;
            _groupService = groupService;

            Processing = new SignalProcessingModel();

            XAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = _globalMinTime,
                    MaxLimit = _globalMaxTime,
                    Labeler = val => val.ToString("0.00")
                }
            };

            YAxes = new Axis[]
            {
                new Axis { Labeler = val => val.ToString("0.00") }
            };

            _viewStart = _globalMinTime;
            _viewEnd = _globalMaxTime;

            UpdateStatistics();
        }

        public async Task AddSignalAsync(
            Signal signal,
            IProgress<double> progress = null,
            CancellationToken token = default)
        {
            if (signal == null) return;

            ResetProcessing();

            if (!SignalGroup.Signals.Contains(signal))
                SignalGroup.Signals.Add(signal);

            RecalculateGlobalTimeRange();

            double dt = (signal.TimeEnd - signal.TimeStart) / (signal.NumberOfPoints - 1);
            double sampleRate = signal.NumberOfPoints / (signal.TimeEnd - signal.TimeStart);

            var raw = await Task.Run(() => _generator.Generate(signal, dt));

            _rawSignals.Add(raw);

            var processed = new double[raw.Length];

            int chunkSize = 100;
            int lastPercent = -1;

            for (int offset = 0; offset < raw.Length; offset += chunkSize)
            {
                token.ThrowIfCancellationRequested();

                int len = Math.Min(chunkSize, raw.Length - offset);

                double[] chunk = new double[len];
                Array.Copy(raw, offset, chunk, 0, len);

                double[] chunkProcessed = await Task.Run(() =>
                    _processor.ProcessSignal(chunk, Processing, sampleRate)
                );

                Array.Copy(chunkProcessed, 0, processed, offset, len);

                int p = (offset + len) * 100 / raw.Length;
                if (p != lastPercent)
                {
                    progress?.Report(p);
                    lastPercent = p;
                }

                await Task.Yield();

                // для теста
                //await Task.Delay(1000, token);
            }

            var points = BuildPoints(signal, processed);

            _seriesPoints.Add(points);

            Series.Add(new LineSeries<ObservablePoint>
            {
                Values = points,
                Name = signal.Name,
                GeometrySize = 0,
                LineSmoothness = 0
            });

            UpdateStatistics();
        }

        public void ApplyProcessing(SignalProcessingModel model)
        {
            if (model == null) return;

            CopyProcessingParameters(model);

            bool isOverlay = model.SelectedMethod == ProcessingMethod.Overlay;

            Series.Clear();
            _seriesPoints.Clear();

            if (_rawSignals.Count == 0)
                return;

            if (isOverlay)
            {
                ApplyOverlayProcessing();
            }
            else
            {
                ApplyRegularProcessing();
            }

            UpdateStatistics();
        }

        private void ApplyRegularProcessing()
        {
            for (int i = 0; i < SignalGroup.Signals.Count; i++)
            {
                var s = SignalGroup.Signals[i];
                var raw = _rawSignals[i];

                double sampleRate = s.NumberOfPoints / (s.TimeEnd - s.TimeStart);

                var processed = _processor.ProcessSignal(raw, Processing, sampleRate);

                var points = BuildPoints(s, processed);

                _seriesPoints.Add(points);
                Series.Add(new LineSeries<ObservablePoint>
                {
                    Values = points,
                    Name = s.Name,
                    GeometrySize = 0,
                    LineSmoothness = 0
                });
            }
        }

        private void ApplyOverlayProcessing()
        {
            double[] combined = _processor.ApplyOverlay(_rawSignals.ToArray());

            var first = SignalGroup.Signals.First();
            double dt = (first.TimeEnd - first.TimeStart) / (first.NumberOfPoints - 1);

            var points = new ObservableCollection<ObservablePoint>(
                combined.Select((y, i) =>
                    new ObservablePoint(first.TimeStart + i * dt, y)));

            _seriesPoints.Add(points);

            string name = string.Join(" + ", SignalGroup.Signals.Select(s => s.Name));

            Series.Add(new LineSeries<ObservablePoint>
            {
                Values = points,
                Name = name,
                GeometrySize = 0,
                LineSmoothness = 0
            });
        }

        private void ResetProcessing()
        {
            Processing.SelectedMethod = ProcessingMethod.None;
            Processing.LowPassCutoff = 0;
            Processing.HighPassCutoff = 0;
            Processing.BandPassLow = 0;
            Processing.BandPassHigh = 0;
            Processing.BandStopLow = 0;
            Processing.BandStopHigh = 0;
            Processing.MovingAverageWindow = 1;
            Processing.ExponentialAlpha = 1;
        }

        private ObservableCollection<ObservablePoint> BuildPoints(Signal s, double[] values)
        {
            double dt = (s.TimeEnd - s.TimeStart) / (values.Length - 1);

            return new ObservableCollection<ObservablePoint>(
                Enumerable.Range(0, values.Length)
                    .Select(i => new ObservablePoint(
                        Math.Round(s.TimeStart + i * dt, 6),
                        Math.Round(values[i], 6)
                    )));
        }

        private void CopyProcessingParameters(SignalProcessingModel model)
        {
            Processing.SelectedMethod = model.SelectedMethod;
            Processing.LowPassCutoff = model.LowPassCutoff;
            Processing.HighPassCutoff = model.HighPassCutoff;
            Processing.BandPassLow = model.BandPassLow;
            Processing.BandPassHigh = model.BandPassHigh;
            Processing.BandStopLow = model.BandStopLow;
            Processing.BandStopHigh = model.BandStopHigh;
            Processing.MovingAverageWindow = model.MovingAverageWindow;
            Processing.ExponentialAlpha = model.ExponentialAlpha;
        }

        private void UpdateViewRange()
        {
            XAxes[0].MinLimit = ViewStart;
            XAxes[0].MaxLimit = ViewEnd;
            XAxes[0].MinStep = Math.Max(0.1, (ViewEnd - ViewStart) / 100);
        }

        private void RecalculateGlobalTimeRange()
        {
            if (SignalGroup.Signals.Count == 0)
            {
                _globalMinTime = 0;
                _globalMaxTime = 10;
            }
            else
            {
                _globalMinTime = SignalGroup.Signals.Min(s => s.TimeStart);
                _globalMaxTime = SignalGroup.Signals.Max(s => s.TimeEnd);

                if (_globalMaxTime - _globalMinTime > MAX_TIME_RANGE)
                    _globalMaxTime = _globalMinTime + MAX_TIME_RANGE;
            }

            OnPropertyChanged(nameof(GlobalMinTime));
            OnPropertyChanged(nameof(GlobalMaxTime));

            var start = Math.Max(_globalMinTime, ViewStart);
            var end = Math.Min(_globalMaxTime, ViewEnd);

            if (end - start > MAX_VIEW_RANGE)
                end = start + MAX_VIEW_RANGE;

            ViewStart = start;
            ViewEnd = end;
        }

        private void UpdateStatistics()
        {
            if (_seriesPoints.Count == 0)
            {
                Mean = Max = Min = 0;
                return;
            }

            double sum = 0;
            double min = double.MaxValue;
            double max = double.MinValue;
            int count = 0;

            foreach (var series in _seriesPoints)
            {
                foreach (var p in series)
                {
                    if (!p.Y.HasValue) continue;
                    if (p.X < ViewStart || p.X > ViewEnd) continue;

                    double y = p.Y.Value;

                    sum += y;
                    count++;

                    if (y < min) min = y;
                    if (y > max) max = y;
                }
            }

            if (count == 0)
            {
                Mean = Max = Min = 0;
                return;
            }

            Mean = sum / count;
            Max = max;
            Min = min;
        }

        public void Clear()
        {
            SignalGroup = new SignalGroup();

            _rawSignals.Clear();

            _seriesPoints.Clear();
            Series.Clear();

            ResetProcessing();

            _globalMinTime = 0;
            _globalMaxTime = 10;
            ViewStart = 0;
            ViewEnd = 10;

            OnPropertyChanged(nameof(GlobalMinTime));
            OnPropertyChanged(nameof(GlobalMaxTime));

            UpdateViewRange();
            UpdateStatistics();
        }
        public async Task SaveSignalGroup(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _signalGroup.Name = name;

            _signalGroup.CreatedAt = DateTime.Now;

            _signalGroup.Signals = SignalGroup.Signals
                .Select(s => new Signal
                {
                    Id = string.IsNullOrEmpty(s.Id) ? Guid.NewGuid().ToString() : s.Id,
                    Name = s.Name,
                    Amplitude = s.Amplitude,
                    Frequency = s.Frequency,
                    Phase = s.Phase,
                    NumberOfPoints = s.NumberOfPoints,
                    TimeStart = s.TimeStart,
                    TimeEnd = s.TimeEnd,
                    Type = s.Type
                })
                .ToList();

            bool success = await _groupService.SaveGroupAsync(_signalGroup);
            MessageBox.Show(success ? "Сохранено!" : "Ошибка при сохранении!");
        }

        public async Task UpdateSignalGroup()
        {
            if (_signalGroup == null)
                return;

            bool success = await _groupService.UpdateGroupAsync(_signalGroup);
            MessageBox.Show(success ? "Сохранено!" : "Ошибка при сохранении!");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}