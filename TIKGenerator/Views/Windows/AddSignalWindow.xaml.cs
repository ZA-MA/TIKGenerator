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

            NameField.Rule = value =>
                string.IsNullOrWhiteSpace(NameField.TextValue)
                    ? (string)Application.Current.Resources["Error_EnterName"]
                    : null;

            AmplitudeField.Rule = value =>
                value == null || value <= 0
                    ? (string)Application.Current.Resources["Error_AmplitudePositive"]
                    : null;

            FrequencyField.Rule = value =>
                value == null || value <= 0
                    ? (string)Application.Current.Resources["Error_FrequencyPositive"]
                    : null;

            PhaseField.Rule = value =>
                value == null
                    ? (string)Application.Current.Resources["Error_PhaseNotEmpty"]
                    : null;

            PointsField.Rule = value =>
                value == null || value < 10
                    ? (string)Application.Current.Resources["Error_PointsMin"]
                    : null;

            TimeStart.Rule = value =>
                value == null || value < 0
                    ? (string)Application.Current.Resources["Error_TimeStartMin"]
                    : null;

            TimeEnd.Rule = value =>
            {
                if (value == null)
                    return (string)Application.Current.Resources["Error_TimeEndMin"];

                if (value < 1)
                    return (string)Application.Current.Resources["Error_TimeEndMin"];

                if (TimeStart.NumberValue != null && value <= TimeStart.NumberValue)
                    return (string)Application.Current.Resources["Error_TimeEndAfterStart"];

                return null;
            };
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NameField.Validate();
                AmplitudeField.Validate();
                FrequencyField.Validate();
                PhaseField.Validate();
                PointsField.Validate();
                TimeStart.Validate();
                TimeEnd.Validate();

                if (!string.IsNullOrEmpty(NameField.Error) ||
                    !string.IsNullOrEmpty(AmplitudeField.Error) ||
                    !string.IsNullOrEmpty(FrequencyField.Error) ||
                    !string.IsNullOrEmpty(PhaseField.Error) ||
                    !string.IsNullOrEmpty(PointsField.Error) ||
                    !string.IsNullOrEmpty(TimeStart.Error) ||
                    !string.IsNullOrEmpty(TimeEnd.Error))
                {
                    MessageBox.Show((string)Application.Current.Resources["Error_FixFields"]);
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
                MessageBox.Show(string.Format((string)Application.Current.Resources["Error_Input"], ex.Message)) ;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
