using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using DBToJsonProject.Controller;
using DBToJsonProject.Controller.SettingManager;
using DBToJsonProject.Models;
using static DBToJsonProject.Controller.SettingManager.DBSettings;
using System.Windows.Input;
using System.Windows.Data;

namespace DBToJsonProject.Views.WorkSpace
{
    class TreeViewLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            TreeViewItem item = (TreeViewItem)value;
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
            return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    /// <summary>
    /// DbSettingToolBox.xaml 的交互逻辑
    /// </summary>
    public partial class DbSettingToolBox : Window
    {
        public event EventHandler<WrongSettingEventArgs> WrongSetting;
        public event EventHandler<StringEventArgs> UnKnowError;
        public DbSettingToolBox()
        {
            InitializeComponent();
            EventManager.RegisterClassHandler(typeof(TextBox), 
                TextBox.PreviewMouseDownEvent, 
                new MouseButtonEventHandler(TextBox_PreviewMouseDown));
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.GotFocusEvent,
                new RoutedEventHandler(TextBox_GotFocus));
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.LostFocusEvent,
                new RoutedEventHandler(TextBox_LostFocus));
        }
        /// <summary>
        /// 将填充树载入View
        /// </summary>
        /// <param name="view"></param>
        /// <param name="detial"></param>
        private void LoadTreeView(TreeView view, JsonEntityDetial detial)
        {
            view.Items.Clear();
            ObservableCollection<PropertyNodeItem> items = new ObservableCollection<PropertyNodeItem>();
            items.Add(PropertyNodeItem.Default);
            items[0].DisplayName = "根节点";
            foreach (IJsonTreeNode n in detial.roots)
            {
                var i = FillItems(n, detial);
                i.Parent = items[0];
                items[0].Childs.Add(i);
            }
            view.ItemsSource = items;
        }
        /// <summary>
        /// 递归填充
        /// </summary>
        /// <param name="root"></param>
        /// <param name="detial"></param>
        /// <returns></returns>
        private PropertyNodeItem FillItems(IJsonTreeNode root, JsonEntityDetial detial)
        {
            PropertyNodeItem item = PropertyNodeItem.Default;
            item.DisplayName = root.DisplayName;
            item.JsonName = root.JsonNodeName;
            item.EntityName = root.DbName;
            item.MultiReleationShip = root.MultiRelated;
            item.HasChildren = root.ChildNodes.Count != 0;
            item.BuildJson = root.BuildSingleFile;
            item.Selectable = root.Selectable;
            item.HasCustomizedSql = root.Sql.HasCustomizeSQLString;
            item.CustomizedSql = root.Sql.CustomizeSQLString;
            item.CustomizedSqlParameters = root.Sql.Params?.ToString();
            item.VirtualNode = root.VirtualNode;
            
            foreach(IJsonTreeNode n in root.ChildNodes.Values)
            {
                var i = FillItems(n, detial);
                i.Parent = item;
                item.Childs.Add(i);
            }
            return item;
        }
        private JsonEntityDetial BuildSetting(PropertyNodeItem root, String constr)
        {
            JsonEntityDetial detial = new JsonEntityDetial();
            detial.DbConnectStr = constr;
            foreach(PropertyNodeItem i in root.Childs)
            {
                detial.roots.Add(BuildNode(i, null));
            }
            return detial;
        }
        private IJsonTreeNode BuildNode(PropertyNodeItem root, IJsonTreeNode parent)
        {
            TreeNode node = new TreeNode(
                root.JsonName,
                root.EntityName,
                root.DisplayName,
                root.MultiReleationShip,
                root.BuildJson,
                root.Selectable,
                root.VirtualNode
                );

            Dictionary<String, IJsonTreeNode> list = new Dictionary<string, IJsonTreeNode>();
            foreach(PropertyNodeItem n in root.Childs)
            {
                list.Add(n.JsonName, BuildNode(n, node));
            }

            node.ChildNodes = list;
            node.Parent = parent;
            node.Sql = new CustomizedSqlDescriber(
                root.HasCustomizedSql, 
                root.CustomizedSql, 
                root.CustomizedSqlParameters, node);
            return node;
        }
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_ImNewImEntity_Click(object sender, RoutedEventArgs e)
        {
            if (Tree_Import.SelectedItem != null)
            {
                PropertyNodeItem i = Tree_Import.SelectedItem as PropertyNodeItem;
                var j = PropertyNodeItem.Default;
                j.Parent = i;
                i.Childs.Add(j);
                i.IsExpanded = true;
                i.HasCustomizedSql = false;
                Tree_Import.Focus();
            }
        }
        private void Btn_NewExEntity_Click(object sender, RoutedEventArgs e)
        {
            if (Tree_Export.SelectedItem != null)
            {
                PropertyNodeItem i = Tree_Export.SelectedItem as PropertyNodeItem;
                var j = PropertyNodeItem.Default;
                j.Parent = i;
                i.Childs.Add(j);
                i.IsExpanded = true;
                i.Selectable = false;
                i.HasCustomizedSql = false;
                Tree_Export.Focus();
            }
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_ImDelImEntity_Click(object sender, RoutedEventArgs e)
        {
            PropertyNodeItem i = Tree_Import.SelectedItem as PropertyNodeItem;
            if (i.Parent != null)
                i.Parent.Childs.Remove(i);
        }

        private void Btn_DelExEntity_Click(object sender, RoutedEventArgs e)
        {
            PropertyNodeItem i = Tree_Export.SelectedItem as PropertyNodeItem;
            if (i.Parent != null)
                i.Parent.Childs.Remove(i);
        }
        /// <summary>
        /// 载入记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTreeView(Tree_Import, DBSettings.Default.ImportRoot);
            LoadTreeView(Tree_Export, DBSettings.Default.ExportRoot);
            Txt_DbConnectStr.Text = Default.DBConnectStr;
            Txt_ExportDbConnectStr.Text = Default.ExportRoot.DbConnectStr;
            Txt_ImportDbConnectStr.Text = Default.ImportRoot.DbConnectStr;
            Txt_UserDbConnectStr.Text = Default.UserRoot.DbConnectStr;
            Txt_UserDbTableName.Text = Default.UserRoot.TableName;
            Txt_UsernameColumnName.Text = Default.UserRoot.UserName;
            Txt_PasswordColumnName.Text = Default.UserRoot.Password;
        }
        /// <summary>
        /// 取消操作，并退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 确定并保存记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Ok_Click(object sender, RoutedEventArgs args)
        {
            Btn_Save_Click(sender, args);
            Btn_Cancel_Click(sender, args);
        }
        /// <summary>
        /// 保存记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Save_Click(object sender, RoutedEventArgs args)
        {
            try
            {
                Save();
            }
            catch (ArgumentException e)
            {
                WrongSetting?.Invoke(this, new WrongSettingEventArgs(e.Message, e.ParamName, e.StackTrace));
            }
            catch (Exception e)
            {
                UnKnowError?.Invoke(this, new StringEventArgs() { Str = e.Message });
            }
        }
        /// <summary>
        /// 保存记录并写入文件
        /// </summary>
        private void Save()
        {
            PropertyNodeItem importRoot = (Tree_Import.ItemsSource as ObservableCollection<PropertyNodeItem>)[0];
            PropertyNodeItem exportRoot = (Tree_Export.ItemsSource as ObservableCollection<PropertyNodeItem>)[0];
            String exdbStr = Txt_ExportDbConnectStr.Text;
            String imdbStr = Txt_ImportDbConnectStr.Text;
            Default.UpdateSetting(Default.UserRoot, BuildSetting(exportRoot, exdbStr), BuildSetting(importRoot, imdbStr));
        }
        /// <summary>
        /// 用户改变了选择项，重新判断该项是否可写
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tree_Import_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }
        private void Tree_Export_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }
        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).PreviewMouseDown += new MouseButtonEventHandler(TextBox_PreviewMouseDown);
        }

        void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).Focus();
            e.Handled = true;
        }

        void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
            (sender as TextBox).PreviewMouseDown -= new MouseButtonEventHandler(TextBox_PreviewMouseDown);
        }
    }
}
