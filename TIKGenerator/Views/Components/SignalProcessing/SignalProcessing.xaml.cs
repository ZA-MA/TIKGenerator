using System.Windows;
using System.Windows.Controls;
using TIKGenerator.Models;
using TIKGenerator.ViewModels;

namespace TIKGenerator.Views.Components.SignalProcessing
{
    public partial class SignalProcessing : UserControl
    {
        public SignalProcessing()
        {
            InitializeComponent();
            DataContext = new SignalProcessingViewModel();
        }

        public SignalProcessingViewModel VM => (SignalProcessingViewModel)DataContext;
    }
}
