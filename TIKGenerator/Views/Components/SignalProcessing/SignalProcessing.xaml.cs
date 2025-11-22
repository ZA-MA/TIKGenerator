using System.Windows;
using System.Windows.Controls;
using TIKGenerator.Models;

namespace TIKGenerator.Views.Components.SignalProcessing
{
    public partial class SignalProcessing : UserControl
    {
        public SignalProcessing()
        {
            InitializeComponent();
        }

        public SignalProcessingOptions Processing
        {
            get => (SignalProcessingOptions)GetValue(ProcessingProperty);
            set => SetValue(ProcessingProperty, value);
        }

        public static readonly DependencyProperty ProcessingProperty =
            DependencyProperty.Register(
                nameof(Processing),
                typeof(SignalProcessingOptions),
                typeof(SignalProcessing),
                new PropertyMetadata(null)
            );

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
