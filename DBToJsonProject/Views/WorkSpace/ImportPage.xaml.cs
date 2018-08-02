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
using DBToJsonProject.Models;
using DBToJsonProject.Models.EventArguments;
using System.Timers;

namespace DBToJsonProject.Views.WorkSpace
{
    /// <summary>
    /// ImportPage.xaml 的交互逻辑
    /// </summary>
    public partial class ImportPage : Page, IWorkPage
    {
        public ImportPage()
        {
            InitializeComponent();

        }


        public event EventHandler CancelExcution;
        public event EventHandler<CmdExecuteArgs> ExecuteCmd;
        public event EventHandler<SelectCollection> SelectionUpdated;

        public void SetupSelections(SelectCollection s)
        {

        }

        public void TaskPostBack(TaskPostBackEventArgs args)
        {
            if (!Btn_ExecuteImport.IsEnabled)
            {
                Txt_LogInfo.Text += args.LogInfo + "\n";
                Txt_LogInfo.ScrollToEnd();
                if (args.Progress == 100)
                {
                    Img_Working.Visibility = Visibility.Hidden;
                    Btn_ExecuteImport.IsEnabled = true;
                }
            }
        }

        public void UpdateFileList(List<FileExpression> files)
        {

        }

        public void UpdatePageInfos(ExportPageInfoEventArgs args)
        {
            
        }

        private void Btn_ExecuteImport_Click(object sender, RoutedEventArgs e)
        {
            Img_Working.Visibility = Visibility.Visible;
            Btn_ExecuteImport.IsEnabled = false;

            ExecuteCmd?.Invoke(this, new CmdExecuteArgs());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //更新时间
            Timer t = new Timer();
            t.Interval = 1000;
            t.Elapsed += T_Elapsed;
            t.Start();
        }
        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Txt_Time.Text = DateTime.Now.ToString();
            });
        }
    }
}
