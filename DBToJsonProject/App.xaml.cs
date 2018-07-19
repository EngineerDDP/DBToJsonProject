using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DBToJsonProject
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Login.LoginWindow login = new Login.LoginWindow();
            WorkSpace.WorkWindow work = new WorkSpace.WorkWindow();
            Controller.ApplicationControl applicationControl = new Controller.ApplicationControl()
            {
                Login = login,
                Work = work
            };
            applicationControl.Startup();
        }
    }
}
