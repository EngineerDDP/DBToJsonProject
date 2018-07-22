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

namespace DBToJsonProject
{
    /// <summary>
    /// ErrorBox.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorBox : Window
    {
        public ErrorBox()
        {
            InitializeComponent();
        }
        public void SetErrorMsg(String msg)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                Txt_ErrorMsg.Text = msg;
            }));
        }
        public void SetErrorTitle(String msg)
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                this.Title = msg;
            }));
        }
    }
}
