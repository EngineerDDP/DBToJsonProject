using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBToJsonProject.Views.Login
{
    /// <summary>
    /// PasswordBoxWithHintText.xaml 的交互逻辑
    /// </summary>
    public partial class PasswordBoxWithHintText : UserControl
    {
        public PasswordBoxWithHintText()
        {
            InitializeComponent();
        }
        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Hit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(PasswordBoxWithHintText), new PropertyMetadata(null, new PropertyChangedCallback(OnHintChanged)));
        private static void OnHintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBoxWithHintText password = d as PasswordBoxWithHintText;
            password.UpdateStates();
        }
        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }
        // Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(string), typeof(PasswordBoxWithHintText), new PropertyMetadata(null));


        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(PasswordBoxWithHintText), new PropertyMetadata(null, new PropertyChangedCallback(OnPasswordChanged)));
        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBoxWithHintText uc = d as PasswordBoxWithHintText;
            uc.UpdateStates();
        }
        private void UpdateStates()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                HintText = Hint;
            }
            else
            {
                HintText = "";
            }
        }
    }
}
