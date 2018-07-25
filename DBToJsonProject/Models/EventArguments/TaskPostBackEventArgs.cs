using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models.EventArguments
{
    public class TaskPostBackEventArgs : EventArgs
    {
        public TaskPostBackEventArgs(int progress, string logInfo, int progressStage, string progressStageDetial)
        {
            Progress = progress;
            LogInfo = logInfo;
            ProgressStage = progressStage;
            ProgressStageDetial = progressStageDetial;
        }

        public Int32 Progress { get; set; }
        public String LogInfo { get; set; }
        public Int32 ProgressStage { get; set; }
        public String ProgressStageDetial { get; set; }
    }
}
