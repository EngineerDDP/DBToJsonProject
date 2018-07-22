using System;
using System.Linq;
using System.Windows;
using DBToJsonProject.Views.Login;
using DBToJsonProject.Views.WorkSpace;
using DBToJsonProject.Models;
using DBToJsonProject.Controller.SettingManager;
using DBToJsonProject.Models.EventArguments;
using DBToJsonProject.Controller.TaskManager;

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
        /// 错误窗口
        /// </summary>
        private ErrorBox errorBox;
        /// <summary>
        /// 数据库设置窗口
        /// </summary>
        private DbSettingToolBox dbSettingbox;
        /// <summary>
        /// 欢迎页
        /// </summary>
        private WelcomePage welcomePage;
        /// <summary>
        /// 导出工作
        /// </summary>
        private ExportPage exportPage;
        /// <summary>
        /// 导入工作
        /// </summary>
        private ImportPage importPage;
        /// <summary>
        /// 用户配置项
        /// </summary>
        private UserSetting userSetting;
        /// <summary>
        /// 初始化资源
        /// </summary>
        public ApplicationControl()
        {
            Login = new LoginWindow();
            Work = new WorkWindow();
            welcomePage = new WelcomePage();
            importPage = new ImportPage();
            exportPage = new ExportPage();
        }
        
        public LoginWindow Login { get => login; set => login = value; }
        public WorkWindow Work { get => work; set => work = value; }
        /// <summary>
        /// 初始化应用并显示第一个窗格
        /// </summary>
        public void Startup()
        {
            Register();
            Login.Show();

            //初始化用户记录
            if (!String.IsNullOrWhiteSpace(AppSetting.Default.ActiveUser))
            {
                userSetting = new UserSetting(AppSetting.Default.ActiveUser);
                //自动填充用户信息
                UseDispatcher(login, () =>
                {
                    login.FillInfo(new UserLoginEventArgs()
                    {
                        Username = userSetting.Name,
                        Password = userSetting.Savedpass,
                        AutomaticLogin = userSetting.AutoLogin,
                        RememberPassword = !String.IsNullOrEmpty(userSetting.Savedpass)
                    });
                });
            }
        }
        /// <summary>
        /// 注册事件监听器
        /// </summary>
        private void Register()
        {
            Login.OnLogin += Login_OnLogin;
            Login.OnExit += Login_OnExit;

            Work.OnWorkSpaceExited += AppExited;
            Work.OnDbSettingRequired += Work_OnDbSettingRequired;
            Work.OnNavigateToExport += NavigateToExport;
            Work.OnNavigateToImPort += NavigateToImPort;
            work.OnNavigateToWelcomePage += NavigateToWelcomePage;
            Work.OnLogout += Logout;

            welcomePage.OnNavigateToExport += NavigateToExport;
            welcomePage.OnNavigateToImPort += NavigateToImPort;

            exportPage.ExecuteExportCmd += ExecuteExportCmd;
        }

        private void ExecuteExportCmd(object sender, ExportCmdExecuteArgs e)
        {
            //执行数据库操作
            var t = new ExportTask();
            t.UpdateProgressInfo += T_UpdateProgressInfo;
            t.Run();
            //记录操作结果
            userSetting.SaveSelections(e.Selections);
        }

        private void T_UpdateProgressInfo(object sender, TaskPostBackEventArgs e)
        {
            e.LogInfo = userSetting.PostLog(e.LogInfo);
            
            UseDispatcher(work, () =>
            {
                work.TaskPostBack(e);
            });
            UseDispatcher(exportPage, () =>
            {
                exportPage.TaskPostBack(e);
            });
        }

        /// <summary>
        /// 将任务添加到UIElement的线程队列
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>
        private void UseDispatcher(UIElement e, Action action)
        {
            e.Dispatcher.Invoke(action);
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Login_OnLogin(object sender, UserLoginEventArgs args)
        {
            UserValidation user = new UserValidation(args.Username, args.Password);
            try
            {
                user.Validate();
            }
            catch (NullReferenceException e)
            {
                PostAnErrorAndExit("数据库异常", new String(e.Message.Take(48).ToArray()));
            }
            //登入成功
            if (user.IsUserValidated)
            {
                userSetting = new UserSetting(user.Username);
                if (args.RememberPassword)
                    userSetting.RememberedPass(args.Password);
                userSetting.AutoLogin = args.AutomaticLogin;
                AppSetting.Default.ActiveUser = args.Username;

                login.Hide();
                work.Show();
                NavigateToWelcomePage(this, new EventArgs());
            }
        }
        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logout(object sender, EventArgs e)
        {
            Login.Show();
            work.Hide();
            userSetting = null;
            
            NavigateToWelcomePage(this, e);
        }
        /// <summary>
        /// 导航至WelcomePage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateToWelcomePage(object sender, EventArgs e)
        {
            UseDispatcher(work, () => { work.SetNavigate(welcomePage); });
        }
        /// <summary>
        /// 导航至ImportPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateToImPort(object sender, EventArgs e)
        {
            UseDispatcher(work, () => { work.SetNavigate(importPage); });
        }
        /// <summary>
        /// 导航至ExportPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateToExport(object sender, EventArgs e)
        {
            SelectCollection selections = DBSettings.Default.BuildSelections();
            userSetting.LoadSelections(ref selections);
            UseDispatcher(work, () => { work.SetNavigate(exportPage); });
            UseDispatcher(exportPage, () =>
            {
                exportPage.SetupSelections(selections);
            });
        }
        /// <summary>
        /// 设置数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

            RefreshWorkWindow();
        }
        /// <summary>
        /// 刷新工作空间
        /// </summary>
        private void RefreshWorkWindow()
        {
            welcomePage = new WelcomePage();
            importPage = new ImportPage();
            exportPage = new ExportPage();
            NavigateToWelcomePage(this, new EventArgs());
        }
        /// <summary>
        /// 程序退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AppExited(object sender, EventArgs args)
        {
            CloseMainWindow();
            SaveData();
        }

        /// <summary>
        /// 关闭主要窗格
        /// </summary>
        private void CloseMainWindow()
        {
            Login?.Close();
            work?.Close();
            dbSettingbox?.Close();
        }
        /// <summary>
        /// 保存设置
        /// </summary>
        private void SaveData()
        {
            DBSettings.Default.Update();
            AppSetting.Default.Update();
            userSetting?.Update();
        }
        private void Login_OnExit(object sender, EventArgs args)
        {
            if(userSetting != null)
            {
                Login.Hide();
                work.Show();
            }
            else
            {
                CloseMainWindow();
            }
        }

        /// <summary>
        /// 设置错误窗格
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        private void SetupErrorBox(String title, String msg)
        {
            if (errorBox == null)
            {
                errorBox = new ErrorBox();
            }
            errorBox.SetErrorMsg(msg);
            errorBox.SetErrorTitle(title);
        }
        /// <summary>
        /// 报普通错误
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        private void PostAnrevivableError(String title,String msg)
        {
            SetupErrorBox(title,msg);
            errorBox.Show();
        }
        /// <summary>
        /// 报严重错误
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        private void PostAnCriticalError(String title, String msg)
        {
            SetupErrorBox(title, msg);
            errorBox.Owner = login.IsActive ? (Window)login : work;
            errorBox.Show();
        }
        /// <summary>
        /// 死球了，再见
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        private void PostAnErrorAndExit(String title, String msg)
        {
            SetupErrorBox(title, msg);
            CloseMainWindow();
            errorBox.Show();
        }
    }
}
