using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Windows;
using System.Windows.Controls;
using TIKGenerator.Models;
using TIKGenerator.Services;
using TIKGenerator.ViewModels;
using TIKGenerator.Views.Windows;

namespace TIKGenerator.Views.Pages
{
    public partial class GeneratorPage : Page
    {
        public GeneratorViewModel Vm { get; }
        private CancellationTokenSource _cts;

        public GeneratorPage(SignalGroup group = null)
        {
            InitializeComponent();

            Vm = App.GeneratorVm;
            DataContext = Vm;

            
            if (group != null)
                LoadGroup(group);
        }

        private async void LoadGroup(SignalGroup group)
        {
            Vm.Clear();

            Vm.SignalGroup = group ?? new SignalGroup();

            foreach (var signal in group.Signals.ToList())
                await Vm.AddSignalAsync(signal);
        }

        private async void AddSignalButton_Click(object sender, RoutedEventArgs e)
        {

            var window = new AddSignalWindow();
            if (window.ShowDialog() != true) return;

            Signal signal = window.Signal;

            ProgressBarControl.Visibility = Visibility.Visible;
            ProgressBarControl.Progress = 0;
            ProgressBarControl.Status = "Генерация...";

            _cts = new CancellationTokenSource();
            ProgressBarControl.CancelCommand = new RelayCommand(_ => _cts.Cancel());

            var progress = new Progress<double>(value =>
            {
                ProgressBarControl.Progress = value;
                ProgressBarControl.Status = $"Генерация... {Math.Round(value)}%";
            });

            try
            {
                await ((GeneratorViewModel)DataContext).AddSignalAsync(signal, progress, _cts.Token);

                ProgressBarControl.Status = "Готово!";
            }
            catch (OperationCanceledException)
            {
                ProgressBarControl.Status = "Отменено!";
            }
            finally
            {
                await Task.Delay(500);
                ProgressBarControl.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Vm.Clear();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Vm.SignalGroup == null || string.IsNullOrEmpty(Vm.SignalGroup.Id))
            {
                var window = new SaveSignalGroup();
                if (window.ShowDialog() != true) return;

                string name = window.GroupName;
                await Vm.SaveSignalGroup(name);
            }
            else
            {
                await Vm.UpdateSignalGroup();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var nav = (Application.Current.MainWindow as MainWindow)?.Nav;
            nav?.SetActiveNavButton("GeneratorPage");
        }
    }
}
