using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Windows;
using System.Windows.Controls;
using TIKGenerator.ViewModels;
using TIKGenerator.Models;

namespace TIKGenerator.Views.Pages
{
    public partial class GeneratorPage : Page
    {
        private SignalChartViewModel Chart;

        public GeneratorPage()
        {
            InitializeComponent();

            Chart = new SignalChartViewModel();
            DataContext = Chart;

            GenerateButton.Click += GenerateButton_Click;
            ClearButton.Click += ClearButton_Click;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double amplitude = double.Parse(AmplitudeBox.Text);
                double frequency = double.Parse(FrequencyBox.Text);
                double phase = double.Parse(PhaseBox.Text);
                int points = int.Parse(PointsBox.Text);
                double tStart = double.Parse(TimeStartBox.Text);
                double tEnd = double.Parse(TimeEndBox.Text);
                SignalType type = (SignalType)((ComboBoxItem)SignalTypeBox.SelectedItem).Tag;

                Chart.GenerateSignal(amplitude, frequency, phase, points, tStart, tEnd, type);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода: " + ex.Message);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Chart.Series = Array.Empty<LiveChartsCore.ISeries>();
        }
    }
}
