using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DBToJsonProject.Views.Login
{
    public class PasswordBoxHelper
    {
        public static bool GetIsPasswordBindingEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPasswordBindingEnabledProperty);
        }
        public static void SetIsPasswordBindingEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPasswordBindingEnabledProperty, value);
        }
        // Using a DependencyProperty as the backing store for IsPasswordBindingEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPasswordBindingEnabledProperty =
            DependencyProperty.RegisterAttached("IsPasswordBindingEnabled", typeof(bool),
            typeof(PasswordBoxHelper),
            new UIPropertyMetadata(false, OnIsPasswordBindingEnableChanged));
        private static void OnIsPasswordBindingEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.PasswordChanged -= passwordBox_PasswordChanged;
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += passwordBox_PasswordChanged;
                }
            }
        }
        static void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (!string.Equals(GetBindedPassword(passwordBox), passwordBox.Password))
            {
                SetBindedPassword(passwordBox, passwordBox.Password);
            }
        }

        public static string GetBindedPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(BindedPasswordProperty);
        }
        public static void SetBindedPassword(DependencyObject obj, string value)
        {
            obj.SetValue(BindedPasswordProperty, value);
        }
        // Using a DependencyProperty as the backing store for BindedPassword.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindedPasswordProperty =
            DependencyProperty.RegisterAttached("BindedPassword",
            typeof(string), typeof(PasswordBoxHelper),
            new UIPropertyMetadata(string.Empty, OnBindedPasswordChanged));
        private static void OnBindedPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            if (passwordBox != null && !string.Equals(GetBindedPassword(passwordBox), passwordBox.Password))
            {
                passwordBox.Password = e.NewValue == null ? string.Empty : e.NewValue.ToString();
            }
        }
    }
}
