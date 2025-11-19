using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.ComponentModel;

namespace TIKGenerator.ViewModels
{
    public class SignalChartViewModel : INotifyPropertyChanged
    {
        private ISeries[] _series;
        public ISeries[] Series
        {
            get => _series;
            set { _series = value; OnPropertyChanged(nameof(Series)); }
        }

        public SignalChartViewModel()
        {
            Series = new ISeries[] { };
        }

        public void GenerateSignal(double amplitude, double frequency, double phase,
                                   int points, double tStart, double tEnd, string type)
        {
            double[] data = new double[points];
            double dt = (tEnd - tStart) / (points - 1);

            for (int i = 0; i < points; i++)
            {
                double t = tStart + i * dt;
                switch (type)
                {
                    case "Синусоида":
                        data[i] = amplitude * Math.Sin(2 * Math.PI * frequency * t + phase);
                        break;
                    case "Меандр":
                        data[i] = amplitude * (Math.Sin(2 * Math.PI * frequency * t + phase) >= 0 ? 1 : -1);
                        break;
                    case "Треугольный":
                        data[i] = 2 * amplitude / Math.PI * Math.Asin(Math.Sin(2 * Math.PI * frequency * t + phase));
                        break;
                    case "Пилообразный":
                        data[i] = 2 * amplitude * (t * frequency - Math.Floor(t * frequency + 0.5));
                        break;
                }
            }

            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = data,
                    LineSmoothness = 0,
                    GeometrySize = 0
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
