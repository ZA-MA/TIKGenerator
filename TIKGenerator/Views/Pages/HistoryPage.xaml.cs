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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TIKGenerator.ViewModels;

namespace TIKGenerator.Views.Pages
{
    public partial class HistoryPage : Page
    {
        public HistoryPageViewModel Vm { get; }

        public HistoryPage()
        {
            InitializeComponent();

            Vm = App.HistoryVm;
            DataContext = Vm;
        }

        private async void HistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            var nav = (Application.Current.MainWindow as MainWindow)?.Nav;
            nav?.SetActiveNavButton("HistoryPage");

            await Vm.LoadAllGroupsAsync();
        }

        private void Previous_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is HistoryPageViewModel vm)
                vm.PreviousPage();
        }

        private void Next_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is HistoryPageViewModel vm)
                vm.NextPage();
        }
    }
}
