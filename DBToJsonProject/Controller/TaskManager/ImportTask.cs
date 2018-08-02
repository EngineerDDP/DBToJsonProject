using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBToJsonProject.Models.EventArguments;
using System.Diagnostics;
using System.IO;

namespace DBToJsonProject.Controller.TaskManager
{
    class ImportTask : ITask
    {
        public int Progress { get; private set; }

        public bool Complete { get; private set; }

        public event EventHandler<StringEventArgs> PostErrorAndAbort;
        public event EventHandler<TaskPostBackEventArgs> UpdateProgressInfo;
        public event EventHandler<FileEventArgs> OnFileOperation;

        /// <summary>
        /// 进程句柄
        /// </summary>
        private Process process;
        /// <summary>
        /// 守护线程
        /// </summary>
        private Task task;

        public ImportTask()
        {
            process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "java";
            process.StartInfo.Arguments = "-jar ./DataSynchronize_IM.jar";
        }

        public void Cancel()
        {
        }

        public void Dispose()
        {
            process.Dispose();
            task.Dispose();
        }

        public void Run()
        {
            task = new Task(Execute);
            task.Start();
        }
        private void Execute()
        {
            Progress = 0;
            try
            {
                process.Start();
                StreamReader sr = process.StandardOutput;
                while (!process.HasExited)
                {
                    string output = sr.ReadLine();
                    string[] info = output.Split('\n');
                    foreach(string i in info)
                    {
                        string[] j = i.Split(' ');
                        Int32 p;
                        if(Int32.TryParse(j[0], out p))
                            UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs(p, j[1], p, j[1]));
                    }
                }
            }
            catch(Exception e)
            {
                PostErrorAndAbort?.Invoke(this, new StringEventArgs() { Str = e.Message });
            }
            finally
            {
                Progress = 100;
                Complete = true;
                UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs(Progress, UpdateStrings.ready, Progress, UpdateStrings.ready));
            }
        }
        
    }
}
