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

namespace DBToJsonProject.Login
{
    public delegate void UserloginEvent(object sender, UserLoginEventArgs args);
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public event UserloginEvent OnLogin;
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Btn_login_Click(object sender, RoutedEventArgs e)
        {
            UserLoginEventArgs args = new UserLoginEventArgs()
            {
                Username = Txt_Username.Text,
                Password = Txt_Password.Text,
                AutomaticLogin = Chk_AutoLogin.IsChecked == true,
                RememberPassword = Chk_RememberPassword.IsChecked == true
            };
            OnLogin?.Invoke(this, args);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
