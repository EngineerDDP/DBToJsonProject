using DBToJsonProject.Models.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBToJsonProject.Controller.TaskManager
{
    class ExportTask
    {
        public event EventHandler<TaskPostBackEventArgs> UpdateProgressInfo;
        public void Run()
        {
            Task t = new Task(Execution);
            t.Start();
        }
        private void Execution()
        {
            Thread.Sleep(1000);
            UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs()
            {
                ProgressStage = "连接数据库",
                ProgressStageDetial = "连接到...",
                Progress = 25,
                LogInfo = "连接"
            });
            Thread.Sleep(1000);
            UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs()
            {
                ProgressStage = "获取数据",
                ProgressStageDetial = "连接到...",
                Progress = 55,
                LogInfo = "获取数据表Export"
            });
            Thread.Sleep(1000);
            UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs()
            {
                ProgressStage = "建立文件",
                ProgressStageDetial = "构建文件",
                Progress = 80,
                LogInfo = "写文件"
            });
            Thread.Sleep(1000);
            UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs()
            {
                ProgressStage = "完成",
                ProgressStageDetial = "操作成功完成",
                Progress = 100,
                LogInfo = "完成操作"
            });
        }
    }
}
