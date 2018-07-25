using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DBToJsonProject.Models;
using DBToJsonProject.Controller.SettingManager;
using DBToJsonProject.Models.EventArguments;
using WpfAnimatedGif;

namespace DBToJsonProject.Views.WorkSpace
{
    /// <summary>
    /// ExportPage.xaml 的交互逻辑
    /// </summary>
    public partial class ExportPage : Page
    {
        private SelectCollection selections;

        public event EventHandler<SelectCollection> SelectionUpdated;           //更新选项集合
        public event EventHandler<ExportCmdExecuteArgs> ExecuteExportCmd;       //执行导出任务
        public event EventHandler CancelExcution;                               //取消任务
        public ExportPage()
        {
            InitializeComponent();
            selections = new SelectCollection();
        }
        public void UpdatePageInfos(ExportPageInfoEventArgs args)
        {
        }
        private void Opt_OpenFileEx_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Opt_CopyFileName_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 页面载入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }
        public void SetupSelections(SelectCollection s)
        {
            selections = s;
            Panel_Selections.ItemsSource = selections.Source;
        }
        public void TaskPostBack(TaskPostBackEventArgs args)
        {
            Txt_LogInfo.Text += args.LogInfo + "\n";
            Txt_LogInfo.ScrollToEnd();
            if(args.Progress == 100)
            {
                Img_Working.Visibility = Visibility.Hidden;
                Btn_ExecuteExport.IsEnabled = true;
                Btn_ResetExportSetting.IsEnabled = true;
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSelectionResult();
        }
        private void UpdateSelectionResult()
        {
            SelectionUpdated?.Invoke(this, selections);
        }

        private void Btn_ResetExportSetting_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0;i < selections.Source.Count;++i)
            {
                for (int j = 0; j < selections.Source[i].Nodes.Count; ++j)
                    selections.Source[i].Nodes[j].IsChecked = false;
            }
            UpdateSelectionResult();
        }

        private void Btn_ExecuteExport_Click(object sender, RoutedEventArgs e)
        {
            String date;
            if (Date_DBDateBegin.SelectedDate == null)
                date = "1990/01/01";
            else
                date = Date_DBDateBegin.SelectedDate?.ToShortDateString();
            ExecuteExportCmd?.Invoke(this, new ExportCmdExecuteArgs()
            {
                Selections = selections,
                SpecifiedQuaryStringArgs = new string[] { String.Format("'{0}'",date) }
            });
            Img_Working.Visibility = Visibility.Visible;
            Btn_ExecuteExport.IsEnabled = false;
            Btn_ResetExportSetting.IsEnabled = false;
        }

        private void CheckBox_CheckAll_Checked(object sender, RoutedEventArgs e)
        {
            var list = ((sender as CheckBox).DataContext as SelectableJsonList);
            for(int i = 0;i < list.Nodes.Count;++i)
            {
                list.Nodes[i].IsChecked = true;
            }
            UpdateSelectionResult();
        }

        private void CheckBox_CheckAll_Unchecked(object sender, RoutedEventArgs e)
        {
            var list = ((sender as CheckBox).DataContext as SelectableJsonList);
            for (int i = 0; i < list.Nodes.Count; ++i)
            {
                list.Nodes[i].IsChecked = false;
            }
            UpdateSelectionResult();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelExcution?.Invoke(this, e);
        }

        private void Txt_LogInfo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
