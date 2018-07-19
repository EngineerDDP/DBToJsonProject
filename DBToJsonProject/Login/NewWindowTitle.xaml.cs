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

namespace DBToJsonProject.Login
{
    public delegate void NewWindowBtnClickEvent(object sender, RoutedEventArgs eventArgs);
    /// <summary>
    /// NewWindowTitle.xaml 的交互逻辑
    /// </summary>
    public partial class NewWindowTitle : UserControl
    {
        public event NewWindowBtnClickEvent CloseBtn_Clicked;
        public event NewWindowBtnClickEvent MinimumBtn_Clicked;
        public NewWindowTitle()
        {
            InitializeComponent();
        }
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            CloseBtn_Clicked?.Invoke(sender, e);
        }
        private void Btn_Minimum_Click(object sender, RoutedEventArgs e)
        {
            MinimumBtn_Clicked?.Invoke(sender, e);
        }
    }
}
