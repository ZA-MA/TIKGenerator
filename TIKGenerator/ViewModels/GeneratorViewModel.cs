using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        private readonly List<ObservableCollection<ObservablePoint>> _seriesPoints = new();

        public ObservableCollection<ISeries> Series { get; } = new();

        public Axis[] XAxes { get; }
        public Axis[] YAxes { get; }

        public SignalProcessingOptions Processing { get; }

        private double _globalMinTime = 0;
        private double _globalMaxTime = 10;

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
        public double Mean
        {
            get => _mean;
            set
            {
                _mean = Math.Round(value, 3);
                OnPropertyChanged(nameof(Mean));
            }
        }

        private double _max;
        public double Max
        {
            get => _max;
            set
            {
                _max = Math.Round(value, 3);
                OnPropertyChanged(nameof(Max));
            }
        }

        private double _min;
        public double Min
        {
            get => _min;
            set
            {
                _min = Math.Round(value, 3);
                OnPropertyChanged(nameof(Min));
            }
        }

 
        private const double MAX_TIME_RANGE = 1000;
        private const double MAX_VIEW_RANGE = 100;

        public GeneratorViewModel(ISignalGeneratorService generator, ISignalProcessingService processor, ISignalGroupService groupService)
        {
            _generator = generator;
            _processor = processor;
            _groupService = groupService;

            Processing = new SignalProcessingOptions();
            Processing.ProcessingApplied += (s, e) =>
            {
                UpdateAllSignals();
                UpdateStatistics();
            };

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

        public async Task AddSignalAsync(Signal signal, IProgress<double> progress = null, CancellationToken token = default)
        {
            if (signal == null) return;

            if(!SignalGroup.Signals.Contains(signal))
                SignalGroup.Signals.Add(signal);

            RecalculateGlobalTimeRange();

            double dt = (signal.TimeEnd - signal.TimeStart) / (signal.NumberOfPoints - 1);
            double sampleRate = signal.NumberOfPoints / (signal.TimeEnd - signal.TimeStart);

            var rawData = await Task.Run(() => _generator.Generate(signal, dt));
            var processedData = new double[rawData.Length];

            int chunkSize = 100;

            for (int offset = 0; offset < rawData.Length; offset += chunkSize)
            {
                token.ThrowIfCancellationRequested();

                int len = Math.Min(chunkSize, rawData.Length - offset);
                double[] chunk = rawData.Skip(offset).Take(len).ToArray();

                double[] processedChunk = await Task.Run(() =>
                    _processor.ProcessSignal(chunk, Processing, sampleRate)
                );

                Array.Copy(processedChunk, 0, processedData, offset, len);

                double percent = (offset + len) * 100.0 / rawData.Length;
                progress?.Report(percent);

                await Task.Yield();

                // задержка для теста
                //await Task.Delay(1000, token);
            }

            var points = new ObservableCollection<ObservablePoint>(
                processedData.Select((y, i) => new ObservablePoint(
                    Math.Round(signal.TimeStart + i * dt, 6),
                    Math.Round(y, 6)
                ))
            );

            _seriesPoints.Add(points);

            var series = new LineSeries<ObservablePoint>
            {
                Values = points,
                Name = signal.Name,
                GeometrySize = 0,
                LineSmoothness = 0
            };

            Series.Add(series);
            UpdateStatistics();
        }

        private void UpdateAllSignals()
        {
            for (int i = 0; i < SignalGroup.Signals.Count; i++)
            {
                var signal = SignalGroup.Signals[i];
                double dt = (signal.TimeEnd - signal.TimeStart) / (signal.NumberOfPoints - 1);
                var data = _generator.Generate(signal, dt);
                double sampleRate = signal.NumberOfPoints / (signal.TimeEnd - signal.TimeStart);
                data = _processor.ProcessSignal(data, Processing, sampleRate);

                var points = new ObservableCollection<ObservablePoint>(
                    data.Select((y, idx) => new ObservablePoint(
                        Math.Round(signal.TimeStart + idx * dt, 6),
                        Math.Round(y, 6)
                    ))
                );
                _seriesPoints[i] = points;

                if (Series[i] is LineSeries<ObservablePoint> series)
                {
                    series.Values = points;
                }
            }
            UpdateViewRange();
            UpdateStatistics();
        }

        private void UpdateViewRange()
        {
            XAxes[0].MinLimit = ViewStart;
            XAxes[0].MaxLimit = ViewEnd;
            XAxes[0].MinStep = Math.Max(0.1, (ViewEnd - ViewStart) / 100);
        }

        public void Clear()
        {
            _signalGroup = new();
            SignalGroup = new();
            _seriesPoints.Clear();
            Series.Clear();
            RecalculateGlobalTimeRange();
            UpdateStatistics();
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
                {
                    _globalMaxTime = _globalMinTime + MAX_TIME_RANGE;
                }
            }

            OnPropertyChanged(nameof(GlobalMinTime));
            OnPropertyChanged(nameof(GlobalMaxTime));

            var newViewStart = Math.Max(_globalMinTime, ViewStart);
            var newViewEnd = Math.Min(_globalMaxTime, ViewEnd);

            if (newViewEnd - newViewStart > MAX_VIEW_RANGE)
            {
                newViewEnd = newViewStart + MAX_VIEW_RANGE;
            }

            ViewStart = newViewStart;
            ViewEnd = newViewEnd;
        }

        private void UpdateStatistics()
        {
            if (_seriesPoints.Count == 0)
            {
                Mean = 0;
                Max = 0;
                Min = 0;
                return;
            }

            var visiblePoints = new List<double>();

            foreach (var points in _seriesPoints)
            {
                var pointsInView = points.Where(p => p.X >= ViewStart && p.X <= ViewEnd && p.Y.HasValue);
                visiblePoints.AddRange(pointsInView.Select(p => p.Y.Value));
            }

            if (visiblePoints.Count == 0)
            {
                Mean = 0;
                Max = 0;
                Min = 0;
                return;
            }

            Mean = Math.Round(visiblePoints.Average(), 6);
            Max = Math.Round(visiblePoints.Max(), 6);
            Min = Math.Round(visiblePoints.Min(), 6);
        }

        public async Task SaveSignalGroup(string name)
        {
            _signalGroup = new SignalGroup();

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

            bool success = await _groupService.UpdateGroupAsync(_signalGroup);
            if (success)
                MessageBox.Show("Cохранено!");
            else
                MessageBox.Show("Ошибка при сохранении!");
        }

        public async Task UpdateSignalGroup()
        {
            if (_signalGroup == null)
                return;

            bool success = await _groupService.UpdateGroupAsync(_signalGroup);
            if (success)
                MessageBox.Show("Cохранено!");
            else
                MessageBox.Show("Ошибка при сохранении!");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}