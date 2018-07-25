using DBToJsonProject.Models.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller.TaskManager
{
    interface ITask
    {
        event EventHandler<StringEventArgs> PostErrorAndAbort;
        event EventHandler<TaskPostBackEventArgs> UpdateProgressInfo;
        int Progress { get; }
        bool Complete { get; }
        void Run();
        void Cancel();
    }
}
