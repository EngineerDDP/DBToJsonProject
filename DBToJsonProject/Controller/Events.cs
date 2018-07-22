using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller
{

    public delegate void WindowExited(object sender, EventArgs args);
    public delegate void RequireChangeDbSetting(object sender, EventArgs args);

    public class WrongSettingEventArgs : EventArgs
    {
        public WrongSettingEventArgs(string wrongTip, string wrongValue, string wrongMsg)
        {
            WrongTip = wrongTip;
            this.wrongValue = wrongValue;
            this.wrongMsg = wrongMsg;
        }

        public string WrongTip { get; set; }
        public string wrongValue { get; set; }
        public string wrongMsg { get; set; }
    }
    public class StringEventArgs : EventArgs
    {
        public String Str { get; set; }
    }
    class Events
    {
    }
}
