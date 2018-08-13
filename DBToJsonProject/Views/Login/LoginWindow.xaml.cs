using DBToJsonProject.Controller;
using DBToJsonProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;

namespace DBToJsonProject.Views.Login
{
    public delegate void UserloginEvent(object sender, UserLoginEventArgs args);
    public class HintTextShowForPassword : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public event EventHandler<UserLoginEventArgs> OnLogin;
        public event EventHandler OnExit;

        public LoginWindow()
        {
            InitializeComponent();
            
        }
        public void FillInfo(UserLoginEventArgs args)
        {
            Txt_Username.Text = args.Username;
            Txt_Password.Password = args.Password;
            Chk_AutoLogin.IsChecked = args.AutomaticLogin;
            Chk_RememberPassword.IsChecked = args.RememberPassword;

            new Task(() =>
            {
                Thread.Sleep(500);
                if (args.AutomaticLogin)
                    this.Dispatcher.Invoke(new Action(PostLoginEvent));
            }).Start();
        }
        public void LoginFailure()
        {
            Txt_loginFailure.Visibility = Visibility.Visible;
        }
        private void PostLoginEvent()
        {
            if (!String.IsNullOrWhiteSpace(Txt_Username.Text) && !String.IsNullOrEmpty(Txt_Password.Password))
            {
                Txt_loginFailure.Visibility = Visibility.Hidden;
                UserLoginEventArgs args = new UserLoginEventArgs()
                {
                    Username = Txt_Username.Text,
                    Password = Txt_Password.Password,
                    AutomaticLogin = Chk_AutoLogin.IsChecked == true,
                    RememberPassword = Chk_RememberPassword.IsChecked == true
                };
                OnLogin?.Invoke(this, args);
            }
        }
        private void Btn_login_Click(object sender, RoutedEventArgs e)
        {
            PostLoginEvent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Btns_Ctrl_MinimumBtn_Clicked(object sender, RoutedEventArgs eventArgs)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Btns_Ctrl_CloseBtn_Clicked(object sender, RoutedEventArgs eventArgs)
        {
            OnExit?.Invoke(sender, eventArgs);
        }

        private void Chk_AutoLogin_Checked(object sender, RoutedEventArgs e)
        {
            PostLoginEvent();
        }
    }
}
