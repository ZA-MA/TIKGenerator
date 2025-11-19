using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TIKGenerator.Views.Components.NavigationPanel
{
    public partial class NavigationPanel : UserControl
    {
        private Button _activeButton;

        public event RoutedEventHandler NavigateGenerator;
        public event RoutedEventHandler NavigateHistory;
        public event RoutedEventHandler NavigateExport;
        public event RoutedEventHandler NavigateImport;

        public NavigationPanel()
        {
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (_activeButton != null)
                _activeButton.Style = (Style)FindResource("NP_ButtonStyle");

            _activeButton = btn;
            _activeButton.Style = (Style)FindResource("NP_ButtonActiveStyle");

            switch ((string)btn.Tag)
            {
                case "GeneratorPage": NavigateGenerator?.Invoke(btn, e); break;
                case "HistoryPage": NavigateHistory?.Invoke(btn, e); break;
                case "ExportPage": NavigateExport?.Invoke(btn, e); break;
                case "ImportPage": NavigateImport?.Invoke(btn, e); break;
            }
        }

        public void SetActiveNavButton(string tag)
        {
            foreach (var child in (this.Content as Panel).Children)
            {
                if (child is Button b)
                {
                    b.Style = (Style)FindResource("NP_ButtonStyle");
                    if ((string)b.Tag == tag)
                    {
                        _activeButton = b;
                        _activeButton.Style = (Style)FindResource("NP_ButtonActiveStyle");
                    }
                }
            }
        }
    }
}
