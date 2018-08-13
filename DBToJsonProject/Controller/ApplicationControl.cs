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
        /// 目标文件列表
        /// </summary>
        private List<FileExpression> files;
        /// <summary>
        /// 初始化资源
        /// </summary>
        public ApplicationControl()
        {
            Login = new LoginWindow();
            Work = new WorkWindow();
            errorBox = new ErrorBox();
            welcomePage = new WelcomePage();
            importPage = new ImportPage();
            exportPage = new ExportPage();

            files = new List<FileExpression>();
        }
        
        public LoginWindow Login { get => login; set => login = value; }
        public WorkWindow Work { get => work; set => work = value; }
        /// <summary>
        /// 初始化应用并显示第一个窗格
        /// </summary>
        public void Startup()
        {
            RegisterWindows();
            SetWindowSize();
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
        /// 设置窗口大小
        /// </summary>
        private void SetWindowSize()
        {
            UseDispatcher(work, () =>
             {
                 work.Width = AppSetting.Default.WindowWidth;
                 work.Height = AppSetting.Default.WindowHeight;
                 work.Left = AppSetting.Default.WindowLeft;
                 Work.Top = AppSetting.Default.WindowTop;
             });
        }

        /// <summary>
        /// 重载页面
        /// </summary>
        private void ReLoadPages()
        {
            welcomePage = new WelcomePage();
            importPage = new ImportPage();
            exportPage = new ExportPage();
            RegisterPages();
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
        /// 执行导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                task.OnFileOperation += Task_OnFileOperation;
                task.Run();
                files.Clear();
            }
            else
            {
                PostAnCriticalError("有任务进行中，无法启动新任务。");
            }
        }
        /// <summary>
        /// 文件操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Task_OnFileOperation(object sender, FileEventArgs e)
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
        private void ExecuteImportCmd(object sender, CmdExecuteArgs e)
        {
            if (task == null || task.Complete)
            {
                task = new ImportTask();

                task.UpdateProgressInfo += T_UpdateProgressInfo;
                task.PostErrorAndAbort += T_PostErrorAndAbort;
                task.OnFileOperation += Task_OnFileOperation;
                task.Run();
                files.Clear();
            }
            else
            {
                PostAnCriticalError("有任务进行中，无法启动新任务。");
            }
        }
        private void ImportPage_CancelExcution(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ImportPage_SelectionUpdated(object sender, SelectCollection e)
        {
            throw new NotImplementedException();
        }
        private void ExportPage_CancelExcution(object sender, EventArgs e)
        {
            (task as ExportTask)?.Cancel();
        }

        /// <summary>
        /// 注册事件监听器
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

            RegisterPages();
        }
        /// <summary>
        /// 记录选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Export_SelectionUpdated(object sender, SelectCollection e)
        {
            userSetting.SaveSelections(e);
        }

        /// <summary>
        /// 任务中止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void T_PostErrorAndAbort(object sender, StringEventArgs e)
        {
            PostAnCriticalError(e.Str);
            task = null;
            T_UpdateProgressInfo(this, new TaskPostBackEventArgs(100, userSetting.PostLog("操作失败 " + e.Str), 100, "准备就绪"));
        }
        /// <summary>
        /// 更新任务进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// 将任务添加到UIElement的线程队列
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>
        private void UseDispatcher(UIElement e, Action action)
        {
            e.Dispatcher.Invoke(action);
        }
        /// <summary>
        /// 将任务添加到UIElement的线程队列
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>
        private void UseDispatcher(System.Windows.Threading.Dispatcher e, Action action)
        {
            e.Invoke(action);
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
        /// 登出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logout(object sender, EventArgs e)
        {
            Login.Show();
            work.Hide();
            userSetting.Update();
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
        /// 设置数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

        private void DbSettingbox_UnKnowError(object sender, StringEventArgs e)
        {
            PostAnCriticalError(e.Str);
        }

        private void DbSettingbox_WrongSetting(object sender, WrongSettingEventArgs e)
        {
            PostAnCriticalError(String.Format("值:{0}，建议:{1}", e.wrongValue, e.WrongTip));
        }

        /// <summary>
        /// 刷新工作空间
        /// </summary>
        private void RefreshWorkWindow()
        {
            ReLoadPages();
            NavigateToWelcomePage(this, new EventArgs());
        }
        /// <summary>
        /// 程序退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
        /// 关闭主要窗格
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
        /// 设置错误窗格
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
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
        /// <param name="title"></param>
        /// <param name="msg"></param>
        private void PostAnCriticalError(String msg)
        {
            SetupErrorBox("错误", msg);
        }
        /// <summary>
        /// 死球了，再见
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        private void PostAnErrorAndExit(String msg)
        {
            SetupErrorBox("严重错误", msg);
            CloseMainWindow();
        }

        public void Dispose()
        {
            task.Dispose();
        }
    }
}
