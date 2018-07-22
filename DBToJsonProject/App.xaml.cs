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
            Controller.IApplicationControl applicationControl = null;
            if (e.Args.Contains("-passlogin"))
                applicationControl = new Controller.ApplicationControl();
            else
                applicationControl = new Controller.ApplicationControl();
            if (e.Args.Contains("-devmode"))
            {

            }
            applicationControl?.Startup();
        }
    }
}
