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
using TIKGenerator.Services;

namespace TIKGenerator.Views.Windows
{
    public partial class ImportWindow : Window
    {
        private readonly ISignalGroupService _service;
        public string SelectedFilePath { get; private set; }

        public ImportWindow(ISignalGroupService service)
        {
            InitializeComponent();
            _service = service;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "CSV|*.csv|JSON|*.json|XML|*.xml";

            if (dlg.ShowDialog() == true)
            {
                PathBox.Text = dlg.FileName;
            }
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            string path = PathBox.Text;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Выберите файл для импорта!");
                return;
            }

            SelectedFilePath = path;
            string ext = Path.GetExtension(path).ToLower();

            try
            {
                SignalGroup group = null;
                switch (ext)
                {
                    case ".csv": group = await ImportCsvAsync(path); break;
                    case ".json": group = await ImportJsonAsync(path); break;
                    case ".xml": group = await ImportXmlAsync(path); break;
                    default:
                        MessageBox.Show("Неподдерживаемый формат файла!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                }

                if (group == null || group.Signals.Count == 0)
                {
                    MessageBox.Show("Файл пустой или некорректный!");
                    return;
                }

                bool result = await _service.SaveGroupAsync(group);
                if (result)
                {
                    MessageBox.Show("Импорт выполнен успешно!");
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при сохранении группы в базу данных.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при импорте: " + ex.Message);
            }
        }

        private async Task<SignalGroup> ImportJsonAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<SignalGroup>(json);
        }

        private async Task<SignalGroup> ImportXmlAsync(string path)
        {
            string xmlText = await File.ReadAllTextAsync(path);
            using var stringReader = new StringReader(xmlText);
            var serializer = new XmlSerializer(typeof(SignalGroup));
            return (SignalGroup)serializer.Deserialize(stringReader);
        }

        private async Task<SignalGroup> ImportCsvAsync(string path)
        {
            var lines = await File.ReadAllLinesAsync(path);
            if (lines.Length < 4) return null;

            var groupInfo = lines[1].Split(',');
            var group = new SignalGroup
            {
                Id = groupInfo[0],
                Name = groupInfo[1],
                CreatedAt = DateTime.Parse(groupInfo[2]),
                Signals = new List<Signal>()
            };

            int signalStart = Array.FindIndex(lines, l => l.StartsWith("SignalId"));
            if (signalStart < 0) return group;

            for (int i = signalStart + 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 9) continue;

                var signal = new Signal
                {
                    Id = parts[0],
                    Name = parts[1],
                    Amplitude = double.Parse(parts[2]),
                    Frequency = double.Parse(parts[3]),
                    Phase = double.Parse(parts[4]),
                    NumberOfPoints = int.Parse(parts[5]),
                    TimeStart = double.Parse(parts[6]),
                    TimeEnd = double.Parse(parts[7]),
                    Type = Enum.Parse<SignalType>(parts[8])
                };

                group.Signals.Add(signal);
            }

            return group;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
