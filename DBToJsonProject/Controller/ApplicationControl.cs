using System;
using System.Linq;
using System.Windows;
using DBToJsonProject.Views.Login;
using DBToJsonProject.Views.WorkSpace;
using DBToJsonProject.Models;
using DBToJsonProject.Controller.SettingManager;

namespace DBToJsonProject.Controller
{
    class ApplicationControl : IApplicationControl
    {
        /// <summary>
        /// 登录窗口
        /// </summary>
        private LoginWindow login;
        /// <summary>
        /// 工作窗口
        /// </summary>
        private WorkWindow work;
        /// <summary>
        /// 用户验证
        /// </summary>
        private UserValidation user;
        /// <summary>
        /// 错误窗口
        /// </summary>
        private ErrorBox errorBox;
        /// <summary>
        /// 数据库设置窗口
        /// </summary>
        private DbSettingToolBox dbSettingbox;
        /// <summary>
        /// 用户配置项
        /// </summary>
        private UserSetting userSetting;
        public ApplicationControl()
        {
            Login = new LoginWindow();
            Work = new WorkWindow();
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
            Work.OnWorkSpaceExited += Work_OnWorkSpaceExited;
            Work.OnDbSettingRequired += Work_OnDbSettingRequired;
            Login.Show();
        }

        private void Work_OnDbSettingRequired(object sender, EventArgs args)
        {
            dbSettingbox = new DbSettingToolBox();
            dbSettingbox.Owner = sender as Window;
            try
            {
                dbSettingbox.ShowDialog();
            }
            catch(InvalidOperationException e)
            {
                PostAnCriticalError("无效操作", e.Message);
                dbSettingbox?.Close();
            }
            dbSettingbox = null;
        }

        private void Work_OnWorkSpaceExited(object sender, EventArgs args)
        {
            CloseMainWindow();
        }

        /// <summary>
        /// 关闭主要窗格
        /// </summary>
        private void CloseMainWindow()
        {
            Login?.Close();
            work?.Close();
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
            errorBox.Show();
        }
        private void PostAnErrorAndExit(String title, String msg)
        {
            SetupErrorBox(title, msg);
            CloseMainWindow();
            errorBox.Show();
        }
    }
}
