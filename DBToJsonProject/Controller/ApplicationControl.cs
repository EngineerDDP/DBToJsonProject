using System;
using System.Linq;
using System.Windows;
using DBToJsonProject.Views.Login;
using DBToJsonProject.Views.WorkSpace;
using DBToJsonProject.Models;
using DBToJsonProject.Controller.SettingManager;
using DBToJsonProject.Models.EventArguments;
using DBToJsonProject.Controller.TaskManager;
using System.Collections.Generic;

namespace DBToJsonProject.Controller
{
    class ApplicationControl : IApplicationControl, IDisposable
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
        /// 任务
        /// </summary>
        private ITask task;
        /// <summary>
        /// 工作目标文件列表
        /// </summary>
        private List<FileExpression> files;
        /// <summary>
        /// 初始化资源
        /// </summary>
        public ApplicationControl()
        {
            files = new List<FileExpression>();
        }
        
        public LoginWindow Login { get => login; set => login = value; }
        public WorkWindow Work { get => work; set => work = value; }
        /// <summary>
        /// 初始化应用并显示第一个窗格
        /// </summary>
        public void Startup()
        {
            LoadWindows();
            RegisterWindows();
            LoadPages();
            RegisterPages();

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
            if(AppSetting.Default.SimpleMode)
                UseDispatcher(work, work.ActivateSimpleMode);
        }
        /// <summary>
        /// 设置窗口大小
        /// </summary>
        private void SetWindowSize()
        {
            AppSetting setting = AppSetting.Default;
            UseDispatcher(work, () =>
             {
                 work.SetPosition(setting.WindowLeft,setting.WindowTop,setting.WindowWidth,setting.WindowHeight);
             });
        }
        /// <summary>
        /// 加载窗口
        /// </summary>
        private void LoadWindows()
        {
            Login = new LoginWindow();

            Work = new WorkWindow();
            SetWindowSize();

            errorBox = new ErrorBox();
        }
        /// <summary>
        /// 注册窗口事件监听器
        /// </summary>
        private void RegisterWindows()
        {
            Login.OnLogin += Login_OnLogin;
            Login.OnExit += AppExited;

            Work.OnWorkSpaceExited += AppExited;
            Work.OnDbSettingRequired += Work_OnDbSettingRequired;
            Work.OnNavigateToExport += NavigateToExport;
            Work.OnNavigateToImPort += NavigateToImPort;
            work.OnNavigateToWelcomePage += NavigateToWelcomePage;
            Work.OnLogout += Logout;
            work.OnSelectSimpleMode += Work_OnSelectSimpleMode;
        }
        /// <summary>
        /// 加载页面
        /// </summary>
        private void LoadPages()
        {
            welcomePage = new WelcomePage();
            importPage = new ImportPage();
            exportPage = new ExportPage();
        }
        /// <summary>
        /// 注册页面事件监听器
        /// </summary>
        private void RegisterPages()
        {
            welcomePage.OnNavigateToExport += NavigateToExport;
            welcomePage.OnNavigateToImPort += NavigateToImPort;

            exportPage.ExecuteCmd += ExecuteExportCmd;
            exportPage.SelectionUpdated += Export_SelectionUpdated;
            exportPage.CancelExcution += ExportPage_CancelExcution;

            importPage.ExecuteCmd += ExecuteImportCmd;
            importPage.SelectionUpdated += ImportPage_SelectionUpdated;
            importPage.CancelExcution += ImportPage_CancelExcution;
        }
        /// <summary>
        /// 为导出任务创建工作线程
        /// </summary>
        private void ExecuteExportCmd(object sender, CmdExecuteArgs e)
        {
            if (task == null || task.Complete)
            {
                String con = String.IsNullOrWhiteSpace(DBSettings.Default.ExportRoot.DbConnectStr) ?
                                DBSettings.Default.DBConnectStr :
                                DBSettings.Default.ExportRoot.DbConnectStr;

                DBSettings.Default.SetupBuildSelections(e.Selections);
                userSetting.ExportImg = e.SelectImgs;
                userSetting.ExportVdo = e.SelectVdos;

                task = new ExportTask(con, DBSettings.Default.ExportRoot, e.SpecifiedQuaryStringArgs, e.SelectVdos, e.SelectImgs);

                task.UpdateProgressInfo += T_UpdateProgressInfo;
                task.PostErrorAndAbort += T_PostErrorAndAbort;
                task.OnFileOperation += T_OnFileOperation;
                task.Run();
                files.Clear();
            }
            else
            {
                PostAnCriticalError("有任务进行中，无法启动新任务。");
            }
        }
        /// <summary>
        /// 为导入任务创建工作线程
        /// </summary>
        private void ExecuteImportCmd(object sender, CmdExecuteArgs e)
        {
            if (task == null || task.Complete)
            {
                task = new ImportTask();

                task.UpdateProgressInfo += T_UpdateProgressInfo;
                task.PostErrorAndAbort += T_PostErrorAndAbort;
                task.OnFileOperation += T_OnFileOperation;
                task.Run();
                files.Clear();
            }
            else
            {
                PostAnCriticalError("有任务进行中，无法启动新任务。");
            }
        }
        /// <summary>
        /// 工作线程进行了文件操作
        /// </summary>
        private void T_OnFileOperation(object sender, FileEventArgs e)
        {
            files.Add(e);
            UseDispatcher(exportPage, () => {
                exportPage.UpdateFileList(files);
            });
            UseDispatcher(importPage, () =>
            {
                importPage.UpdateFileList(files);
            });
        }
        /// <summary>
        /// 工作线程报告任务中止
        /// </summary>
        private void T_PostErrorAndAbort(object sender, ExceptionEventArgs e)
        {
            userSetting.PostLog("Stack trace :" + e.E.StackTrace);
            PostAnCriticalError(e.Str);
            task = null;
            T_UpdateProgressInfo(this, new TaskPostBackEventArgs(100, "操作失败 " + e.Str, 100, "准备就绪"));
        }
        /// <summary>
        /// 工作线程更新任务进度
        /// </summary>
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
            UseDispatcher(importPage, () =>
            {
                importPage.TaskPostBack(e);
            });
        }
        /// <summary>
        /// 导入页面，取消操作
        /// </summary>
        private void ImportPage_CancelExcution(object sender, EventArgs e)
        {
            (task as ImportTask)?.Cancel();
        }
        /// <summary>
        /// 导入页面，更新用户选项
        /// </summary>
        private void ImportPage_SelectionUpdated(object sender, SelectCollection e)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 导出页面，取消操作
        /// </summary>
        private void ExportPage_CancelExcution(object sender, EventArgs e)
        {
            (task as ExportTask)?.Cancel();
        }
        /// <summary>
        /// 使用简单模式
        /// </summary>
        private void Work_OnSelectSimpleMode(object sender, Boolean e)
        {
            AppSetting.Default.SimpleMode = e;
        }

        /// <summary>
        /// 记录导出页面选项
        /// </summary>
        private void Export_SelectionUpdated(object sender, SelectCollection e)
        {
            userSetting.SaveSelections(e);
        }

        /// <summary>
        /// 将任务添加到UIElement的线程队列
        /// </summary>
        /// <param name="e">Dispatcher</param>
        /// <param name="action">要执行的动作</param>
        private void UseDispatcher(UIElement e, Action action)
        {
            e.Dispatcher.Invoke(action);
        }
        /// <summary>
        /// 将任务添加到Dispatcher的线程队列
        /// </summary>
        /// <param name="e">Dispatcher</param>
        /// <param name="action">要执行的动作</param>
        private void UseDispatcher(System.Windows.Threading.Dispatcher e, Action action)
        {
            e.Invoke(action);
        }
        /// <summary>
        /// 登录窗口登录事件
        /// </summary>
        private void Login_OnLogin(object sender, UserLoginEventArgs args)
        {
            UserValidation user = new UserValidation(args.Username, args.Password);
            try
            {
                user.Validate();
            }
            catch (NullReferenceException e)
            {
                PostAnErrorAndExit(new String(e.Message.Take(48).ToArray()));
            }
            //登入成功
            if (user.IsUserValidated)
            {
                userSetting = new UserSetting(user.Username);
                if (args.RememberPassword)
                    userSetting.RememberedPass(args.Password);
                else
                    userSetting.ForgetPass();
                userSetting.AutoLogin = args.AutomaticLogin;
                AppSetting.Default.ActiveUser = args.Username;

                login.Hide();
                UseDispatcher(work, () =>
                {
                    work.SetUsername(userSetting.Name);
                });

                RefreshWorkWindow();
                work.Show();
            }
            else
            {
                UseDispatcher(login, login.LoginFailure);
            }
        }
        /// <summary>
        /// 工作页面登出事件
        /// </summary>
        private void Logout(object sender, EventArgs e)
        {
            Login.Show();
            work.Hide();
            userSetting.Update();
            userSetting = null;
            
            NavigateToWelcomePage(this, e);
        }
        /// <summary>
        /// 导航事件，至起始页
        /// </summary>
        private void NavigateToWelcomePage(object sender, EventArgs e)
        {
            UseDispatcher(work, () => { work.SetNavigate(welcomePage); });
        }
        /// <summary>
        /// 导航事件，至导入页面
        /// </summary>
        private void NavigateToImPort(object sender, EventArgs e)
        {
            UseDispatcher(work, () => { work.SetNavigate(importPage); });
        }
        /// <summary>
        /// 导航事件，至导出页面
        /// </summary>
        private void NavigateToExport(object sender, EventArgs e)
        {
            //初始化选项集合
            SelectCollection selections = DBSettings.Default.BuildSelections();
            //使用用户设置预选集合
            userSetting.LoadSelections(ref selections);
            //导航至ExportPage
            UseDispatcher(work, () => { work.SetNavigate(exportPage); });
            UseDispatcher(exportPage, () =>
            {
                //更新选择集合
                exportPage.SetupSelections(selections);
                exportPage.UpdatePageInfos(new ExportPageInfoEventArgs()
                {
                    VdoSelected = userSetting.ExportVdo,
                    ImgSelected = userSetting.ExportImg
                });
            });
        }
        /// <summary>
        /// 请求弹出数据库设置属性窗口
        /// </summary>
        private void Work_OnDbSettingRequired(object sender, EventArgs args)
        {
            dbSettingbox = new DbSettingToolBox();
            dbSettingbox.Owner = sender as Window;
            dbSettingbox.WrongSetting += DbSettingbox_WrongSetting;
            dbSettingbox.UnKnowError += DbSettingbox_UnKnowError;
            try
            {
                dbSettingbox.ShowDialog();
            }
            catch(InvalidOperationException e)
            {
                PostAnCriticalError(e.Message);
                dbSettingbox?.Close();
            }
            dbSettingbox = null;

            RefreshWorkWindow();
        }
        /// <summary>
        /// 属性设置窗口出现未知错误
        /// </summary>
        private void DbSettingbox_UnKnowError(object sender, ExceptionEventArgs e)
        {
            userSetting.PostLog("Stack trace :" + e.E.StackTrace);
            PostAnCriticalError(e.Str);
        }
        /// <summary>
        /// 属性设置窗口通知属性设置错误
        /// </summary>
        private void DbSettingbox_WrongSetting(object sender, WrongSettingEventArgs e)
        {
            PostAnCriticalError(String.Format("值:{0}，建议:{1}", e.wrongValue, e.WrongTip));
        }
        /// <summary>
        /// 刷新工作空间
        /// </summary>
        private void RefreshWorkWindow()
        {
            LoadPages();
            RegisterPages();

            NavigateToWelcomePage(this, new EventArgs());
        }
        /// <summary>
        /// 程序退出
        /// </summary>
        private void AppExited(object sender, EventArgs args)
        {
            task?.Cancel();
            SaveWindowSize();
            CloseMainWindow();
            SaveData();
        }
        private void SaveWindowSize()
        {
            AppSetting.Default.WindowLeft = work.Left;
            AppSetting.Default.WindowTop = work.Top;
            AppSetting.Default.WindowHeight = work.Height;
            AppSetting.Default.WindowWidth = work.Width;
        }
        /// <summary>
        /// 关闭主要窗口
        /// </summary>
        private void CloseMainWindow()
        {
            Login?.Close();
            work?.Close();
            dbSettingbox?.Close();
            errorBox?.Close();
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

        /// <summary>
        /// 设置错误窗口
        /// </summary>
        private void SetupErrorBox(String title, String msg)
        {
            try
            {
                
                UseDispatcher(App.Current.Dispatcher, () =>
                {
                    errorBox.SetErrorMsg(msg);
                    errorBox.SetErrorTitle(title);
                    errorBox.Owner = work;
                    errorBox.Show();
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 报严重错误
        /// </summary>
        /// <param name="msg">错误信息</param>
        private void PostAnCriticalError(String msg)
        {
            SetupErrorBox("错误", msg);
        }
        /// <summary>
        /// 死球了，再见
        /// </summary>
        /// <param name="msg">错误信息</param>
        private void PostAnErrorAndExit(String msg)
        {
            SetupErrorBox("严重错误", msg);
            CloseMainWindow();
        }
        /// <summary>
        /// 析构方法
        /// </summary>
        public void Dispose()
        {
            task.Dispose();
        }
    }
}
