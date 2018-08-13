using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DBToJsonProject.Models;
using DBToJsonProject.Controller.SettingManager;
using DBToJsonProject.Models.EventArguments;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;

namespace DBToJsonProject.Views.WorkSpace
{
    public class SelectAll : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<SelectableJsonNode> val = (value as SelectableJsonList).Nodes;
            if(val != null)
            {
                foreach (SelectableJsonNode n in val)
                    if (!n.IsChecked)
                        return false;
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// ExportPage.xaml 的交互逻辑
    /// </summary>
    public partial class ExportPage : Page, IWorkPage
    {
        private SelectCollection selections;

        public event EventHandler<SelectCollection> SelectionUpdated;           //更新选项集合
        public event EventHandler<CmdExecuteArgs> ExecuteCmd;       //执行导出任务
        public event EventHandler CancelExcution;                               //取消任务

        public ExportPage()
        {
            InitializeComponent();

            selections = new SelectCollection();
            files = new ObservableCollection<FileExpression>();
            Lst_TargetFiles.ItemsSource = files;
        }
        public void UpdatePageInfos(ExportPageInfoEventArgs args)
        {
            Chk_ExportVdo.IsChecked = args.VdoSelected;
            Chk_ExportImg.IsChecked = args.ImgSelected;
        }
        private ObservableCollection<FileExpression> files;
        public void UpdateFileList(List<FileExpression> e)
        {
            files.Clear();
            foreach (FileExpression f in e)
                files.Add(f);
        }
        private void Opt_OpenFileEx_Click(object sender, RoutedEventArgs e)
        {
            String path = (Lst_TargetFiles.SelectedItem as FileExpression).Path;
            path = Environment.CurrentDirectory + "\\" + path;
            System.Diagnostics.Process.Start(path);
        }

        private void Opt_CopyFileName_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText((Lst_TargetFiles.SelectedItem as FileExpression).FileName);
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
            Panel_Selections.SelectedIndex = 0;
        }
        public void TaskPostBack(TaskPostBackEventArgs args)
        {
            if (!Btn_ExecuteExport.IsEnabled)
            {
                Txt_LogInfo.Text += args.LogInfo + "\n";
                Txt_LogInfo.ScrollToEnd();
                if (args.Progress == 100)
                {
                    Img_Working.Visibility = Visibility.Hidden;
                    Btn_ExecuteExport.IsEnabled = true;
                    Btn_ResetExportSetting.IsEnabled = true;
                }
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

            ExecuteCmd?.Invoke(this, new CmdExecuteArgs(selections, 
                                        new string[] { String.Format("'{0}'", date) }, 
                                        Chk_ExportImg.IsChecked.Value, 
                                        Chk_ExportVdo.IsChecked.Value));

            Img_Working.Visibility = Visibility.Visible;
            Btn_ExecuteExport.IsEnabled = false;
            Btn_ResetExportSetting.IsEnabled = false;
        }

        private void CheckBox_CheckAll_Checked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(e.OriginalSource);
            var list = ((sender as CheckBox).DataContext as SelectableJsonList);
            for(int i = 0;i < list.Nodes.Count;++i)
            {
                list.Nodes[i].IsChecked = true;
            }
            UpdateSelectionResult();
        }

        private void CheckBox_CheckAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(e.OriginalSource);
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
    }
}
