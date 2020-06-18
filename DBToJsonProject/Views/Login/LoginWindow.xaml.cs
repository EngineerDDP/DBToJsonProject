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
    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean? val = value as Boolean?;
            if (val != true)
                return Visibility.Hidden;
            return Visibility.Visible;
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

        private LoginInfo loginInfo;

        public LoginWindow()
        {
            InitializeComponent();

            loginInfo = new LoginInfo();
            Txt_loginFailure.DataContext = loginInfo;
            AppNameTitle.Text = Controller.SettingManager.AppSetting.Default.AppName;
          
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
            loginInfo.Message = "登录失败，请检查用户名和密码是否正确!";
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
                loginInfo.Message = "正在登录...";
            }
            else if(String.IsNullOrWhiteSpace(Txt_Username.Text))
            {
                loginInfo.Message = "请输入用户名!";
                loginInfo.IsShown = true;
            }
            else if(String.IsNullOrEmpty(Txt_Password.Password))
            {
                loginInfo.Message = "请输入密码!";
                loginInfo.IsShown = true;
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
