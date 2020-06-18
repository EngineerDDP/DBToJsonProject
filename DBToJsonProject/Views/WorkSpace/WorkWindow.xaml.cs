using DBToJsonProject.Controller;
using DBToJsonProject.Models.EventArguments;
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
        public event EventHandler OnNavigateToExport;
        public event EventHandler OnNavigateToImPort;
        public event EventHandler OnNavigateToWelcomePage;
        public event EventHandler OnLogout;
        public event EventHandler<Boolean> OnSelectSimpleMode;
        

        public WorkWindow()
        {
            InitializeComponent();

            this.Title = Controller.SettingManager.AppSetting.Default.AppName;
        }
        public void ActivateSimpleMode()
        {
            Mnu_Setting.Visibility = Visibility.Hidden;
        }
        public void SetPosition(double left, double top, double width, double height)
        {
            if (left + width > SystemParameters.WorkArea.Width)
                return;
            if (top + height > SystemParameters.WorkArea.Height)
                return;
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }
        public void SetNavigate(Page page)
        {
            Frame_MainWorkSpace.Navigate(page);
        }
        public void SetUsername(String name)
        {
            Txt_LoginUser.Text = name;
        }
        public void TaskPostBack(TaskPostBackEventArgs args)
        {
            Progress_Status.Value = args.Progress;
            Txt_StatusA.Text = args.ProgressStageDetial;
            Progress_Status_B.Value = args.ProgressStage;
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
            OnNavigateToImPort?.Invoke(this, e);
        }

        private void Opt_ExportJob_Click(object sender, RoutedEventArgs e)
        {
            OnNavigateToExport?.Invoke(this, e);
        }

        private void Opt_SimpleMode_Click(object sender, RoutedEventArgs e)
        {
            OnSelectSimpleMode?.Invoke(this, true);
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
            System.Diagnostics.Process.Start("help.rtf");
        }

        private void Opt_About_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }

        private void Opt_ToWelcomePage_Click(object sender, RoutedEventArgs e)
        {
            OnNavigateToWelcomePage?.Invoke(this, e);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OnWorkSpaceExited?.Invoke(this, e);
        }

        private void Opt_Logout_Click(object sender, RoutedEventArgs e)
        {
            OnLogout?.Invoke(this, e);
        }

        private void Opt_SimpleMode_Unchecked(object sender, RoutedEventArgs e)
        {
            OnSelectSimpleMode?.Invoke(this, false);
        }
    }
}
