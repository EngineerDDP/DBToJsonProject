using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DBToJsonProject.Views.WorkSpace
{
    /// <summary>
    /// WelcomePage.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomePage : Page
    {
        public event EventHandler OnNavigateToExport;
        public event EventHandler OnNavigateToImPort;
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void Link_NewImportJob_Click(object sender, RoutedEventArgs e)
        {
            OnNavigateToImPort?.Invoke(this, e);
        }

        private void Link_NewEXportJob_Click(object sender, RoutedEventArgs e)
        {
            OnNavigateToExport?.Invoke(this, e);
        }

        private void Link_Help_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_AddDevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_RemoveDevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_CopyID_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
