using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller
{

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
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception e, string str)
        {
            E = e;
            Str = str;
        }

        public Exception E { get; set; }
        public String Str { get; set; }
    }
    public class FileEventArgs
    {
        public FileEventArgs(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }

        public String FileName { get; set; }
        public String FilePath { get; set; }
    }
}
