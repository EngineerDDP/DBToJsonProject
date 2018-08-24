using DBToJsonProject.Models.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller.TaskManager
{
    interface ITask : IDisposable
    {
        event EventHandler<ExceptionEventArgs> PostErrorAndAbort;
        event EventHandler<TaskPostBackEventArgs> UpdateProgressInfo;
        event EventHandler<FileEventArgs> OnFileOperation;
        int Progress { get; }
        bool Complete { get; }
        void Run();
        void Cancel();
    }
}
