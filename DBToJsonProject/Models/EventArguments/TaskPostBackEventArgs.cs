using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models.EventArguments
{
    public class TaskPostBackEventArgs : EventArgs
    {
        public Int32 Progress { get; set; }
        public String LogInfo { get; set; }
        public String ProgressStage { get; set; }
        public String ProgressStageDetial { get; set; }
    }
}
