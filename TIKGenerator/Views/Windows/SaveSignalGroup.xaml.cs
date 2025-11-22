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

namespace TIKGenerator.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SaveSignalGroup.xaml
    /// </summary>
    public partial class SaveSignalGroup : Window
    {
        public string GroupName { get; private set; }

        public SaveSignalGroup()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GroupName = NameField.TextValue;

                this.DialogResult = true;
            }
            catch
            {

            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
