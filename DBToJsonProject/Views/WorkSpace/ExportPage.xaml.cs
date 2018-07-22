using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DBToJsonProject.Models;
using DBToJsonProject.Controller.SettingManager;
using DBToJsonProject.Models.EventArguments;

namespace DBToJsonProject.Views.WorkSpace
{
    /// <summary>
    /// ExportPage.xaml 的交互逻辑
    /// </summary>
    public partial class ExportPage : Page
    {
        private SelectCollection selections;
        public event EventHandler<SelectCollection> SelectionUpdated;
        public event EventHandler<ExportCmdExecuteArgs> ExecuteExportCmd;
        public ExportPage()
        {
            InitializeComponent();
            selections = new SelectCollection();
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
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSelectionResult();
        }
        private void UpdateSelectionResult()
        {
            Txt_SelectedResult.Clear();
            foreach (SelectableJsonList l in selections.Source)
            {
                Txt_SelectedResult.Text += String.Format("{0}:\n", l.Name);
                foreach (SelectableJsonNode n in l.Nodes)
                {
                    if(n.IsChecked)
                        Txt_SelectedResult.Text += String.Format("\t[{0}]\t", n.Name);
                }
                Txt_SelectedResult.Text += "\n";
            }
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
            ExecuteExportCmd?.Invoke(this, new ExportCmdExecuteArgs()
            {
                Selections = selections,
                SpecifiedQuaryString = ""
            });
        }
    }
}
