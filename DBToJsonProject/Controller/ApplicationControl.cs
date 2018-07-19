using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBToJsonProject.Login;
using System.Windows;
using DBToJsonProject.WorkSpace;

namespace DBToJsonProject.Controller
{
    class ApplicationControl
    {
        /// <summary>
        /// 登录窗口
        /// </summary>
        private LoginWindow login;
        /// <summary>
        /// 工作窗口
        /// </summary>
        private WorkSpace.WorkWindow work;
        private UserValidation user;
        private ErrorBox errorBox;
        public ApplicationControl()
        {
            
        }
        
        public LoginWindow Login { get => login; set => login = value; }
        public WorkWindow Work { get => work; set => work = value; }
        /// <summary>
        /// 初始化应用并显示第一个窗格
        /// </summary>
        public void Startup()
        {
            Login.OnLogin += Login_OnLogin;
            Login.OnExit += Login_OnExit;
            Login.Show();
        }
        /// <summary>
        /// 关闭主要窗格
        /// </summary>
        private void CloseMainWindow()
        {
            Login.Close();
            work.Close();
        }

        private void Login_OnExit(object sender, EventArgs args)
        {
            if(user != null && user.IsUserValidated)
            {
                Login.Hide();
                work.Show();
            }
            else
            {
                CloseMainWindow();
            }
        }

        private void Login_OnLogin(object sender, UserLoginEventArgs args)
        {
            user = new UserValidation(args.Username, args.Password);
            try
            {
                user.Validate();
            }
            catch(NullReferenceException e)
            {
                PostAnErrorAndExit("数据库异常", new String(e.Message.Take(48).ToArray()));
            }
            if(user.IsUserValidated)
            {
                login.Hide();
                work.Show();
            }
        }
        private void SetupErrorBox(String title, String msg)
        {
            if (errorBox == null)
            {
                errorBox = new ErrorBox();
            }
            errorBox.SetErrorMsg(msg);
            errorBox.SetErrorTitle(title);
        }
        private void PostAnrevivableError(String title,String msg)
        {
            SetupErrorBox(title,msg);
            errorBox.Show();
        }
        private void PostAnCriticalError(String title, String msg)
        {
            SetupErrorBox(title, msg);
            errorBox.Owner = login.IsActive ? (Window)login : work;
        }
        private void PostAnErrorAndExit(String title, String msg)
        {
            SetupErrorBox(title, msg);
            CloseMainWindow();
            errorBox.Show();
        }
    }
}
