using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DBToJsonProject.Models;
using DBToJsonProject.Controller.SettingManager;

namespace DBToJsonProject.Views.WorkSpace
{
    /// <summary>
    /// ExportPage.xaml 的交互逻辑
    /// </summary>
    public partial class ExportPage : Page
    {
        private Selections selections;
        public event EventHandler<Selections> SelectionUpdated;
        public ExportPage()
        {
            InitializeComponent();
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
            selections = DBSettings.Default.BuildSelections();
            Panel_Selections.ItemsSource = selections.Source;
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
    }
}
