using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TIKGenerator.Models;

namespace TIKGenerator.Views.Windows
{
    public partial class AddSignalWindow : Window
    {
        public Signal Signal { get; private set; }

        public AddSignalWindow()
        {
            InitializeComponent();

            NameField.Rule = value => string.IsNullOrWhiteSpace(NameField.TextValue) ? "Введите название" : null;

            AmplitudeField.Rule = value => value == null || value <= 0 ? "Амплитуда должна быть > 0" : null;

            FrequencyField.Rule = value =>
                value == null || value <= 0
                    ? "Частота должна быть > 0"
                    : null;

            PhaseField.Rule = value =>
                value == null
                    ? "Фаза не может быть пустой"
                    : null;

            PointsField.Rule = value =>
                value == null || value < 10
                    ? "Количество точек должно быть ≥ 10"
                    : null;

            TimeStart.Rule = value =>
                value == null || value >= 0
                    ? "Время начала должно быть ≥ 0"
                    : null;

            TimeEnd.Rule = value =>
                value == null || value >= 1
                    ? "Время начала должно быть ≥ 1"
                    : null;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(NameField.Error) ||
                    !string.IsNullOrEmpty(AmplitudeField.Error) ||
                    !string.IsNullOrEmpty(FrequencyField.Error) ||
                    !string.IsNullOrEmpty(PhaseField.Error) ||
                    !string.IsNullOrEmpty(PointsField.Error))
                {
                    MessageBox.Show("Исправьте ошибки в полях ввода.");
                    return;
                }

                Signal = new Signal
                {
                    Name = NameField.TextValue,
                    Amplitude = AmplitudeField.NumberValue ?? 0,
                    Frequency = FrequencyField.NumberValue ?? 0,
                    Phase = PhaseField.NumberValue ?? 0,
                    NumberOfPoints = (int)(PointsField.NumberValue ?? 0),
                    TimeStart = TimeStart.NumberValue ?? 0,
                    TimeEnd = TimeEnd.NumberValue ?? 0,
                    Type = (SignalType)((ComboBoxItem)SignalTypeBox.SelectedItem).Tag
                };

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ввода: " + ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
