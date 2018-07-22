using DBToJsonProject.Controller;
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
using System.Windows.Shapes;

namespace DBToJsonProject.Views.WorkSpace
{
    /// <summary>
    /// WorkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WorkWindow : Window
    {
        public event EventHandler OnWorkSpaceExited;
        public event EventHandler OnDbSettingRequired;

        public WorkWindow()
        {
            InitializeComponent();
        }

        private void Opt_SeeLogInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_Exit_Click(object sender, RoutedEventArgs e)
        {
            OnWorkSpaceExited?.Invoke(this, e);
        }

        private void Opt_ImportJob_Click(object sender, RoutedEventArgs e)
        {
            Frame_MainWorkSpace.Navigate(new ImportPage());
        }

        private void Opt_ExportJob_Click(object sender, RoutedEventArgs e)
        {
            Frame_MainWorkSpace.Navigate(new ExportPage());
        }

        private void Opt_SimpleMode_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_JobOption_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_DBOption_Click(object sender, RoutedEventArgs e)
        {
            OnDbSettingRequired?.Invoke(this, e);
        }

        private void Opt_Manual_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_About_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_ToWelcomePage_Click(object sender, RoutedEventArgs e)
        {
            Frame_MainWorkSpace.Navigate(new WelcomePage());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OnWorkSpaceExited?.Invoke(this, e);
        }

        private void Opt_Logout_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
