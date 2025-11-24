using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using TIKGenerator.Models;

namespace TIKGenerator.Views.Windows
{
    public partial class ExportWindow : Window
    {
        private readonly SignalGroup _group;

        public ExportWindow(SignalGroup group)
        {
            InitializeComponent();
            _group = group;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            string format = (FormatBox.SelectedItem as ComboBoxItem).Content.ToString();

            switch (format)
            {
                case "CSV": dlg.Filter = "CSV|*.csv"; break;
                case "JSON": dlg.Filter = "JSON|*.json"; break;
                case "XML": dlg.Filter = "XML|*.xml"; break;
            }

            if (dlg.ShowDialog() == true)
            {
                PathBox.Text = dlg.FileName;
            }
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            string path = PathBox.Text;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Укажите путь и имя файла!");
                return;
            }

            string format = (FormatBox.SelectedItem as ComboBoxItem).Content.ToString();

            try
            {
                switch (format)
                {
                    case "CSV": await ExportCsvAsync(_group, path); break;
                    case "JSON": await ExportJsonAsync(_group, path); break;
                    case "XML": await ExportXmlAsync(_group, path); break;
                }

                MessageBox.Show("Экспорт выполнен!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportCsvAsync(SignalGroup group, string path)
        {
            var sb = new StringBuilder();

            sb.AppendLine("GroupId,GroupName,CreatedAt");
            sb.AppendLine($"{group.Id},{group.Name},{group.CreatedAt:O}");

            sb.AppendLine();
            sb.AppendLine("SignalId,Name,Amplitude,Frequency,Phase,NumberOfPoints,TimeStart,TimeEnd,Type");
            foreach (var s in group.Signals)
            {
                sb.AppendLine($"{s.Id},{s.Name},{s.Amplitude},{s.Frequency},{s.Phase},{s.NumberOfPoints},{s.TimeStart},{s.TimeEnd},{s.Type}");
            }

            await File.WriteAllTextAsync(path, sb.ToString());
        }

        private async Task ExportJsonAsync(SignalGroup group, string path)
        {
            var json = JsonSerializer.Serialize(group, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }

        private async Task ExportXmlAsync(SignalGroup group, string path)
        {
            var serializer = new XmlSerializer(typeof(SignalGroup));
            await using var writer = new StreamWriter(path);
            serializer.Serialize(writer, group);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
