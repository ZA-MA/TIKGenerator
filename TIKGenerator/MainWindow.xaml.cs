using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TIKGenerator.Views;
using TIKGenerator.Views.Pages;

namespace TIKGenerator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Content = new GeneratorPage();
            Nav.SetActiveNavButton("GeneratorPage");

            Nav.NavigateGenerator += (s, e) => MainFrame.Content = new GeneratorPage();
            Nav.NavigateHistory += (s, e) => MainFrame.Content = new HistoryPage();
            Nav.NavigateExport += (s, e) => MainFrame.Content = new ExportPage();
            Nav.NavigateImport += (s, e) => MainFrame.Content = new ImportPage();
        }
    }
}