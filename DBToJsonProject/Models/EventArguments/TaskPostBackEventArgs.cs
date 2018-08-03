using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models.EventArguments
{
    /// <summary>
    /// 更新任务执行进度的消息参数类
    /// </summary>
    public class TaskPostBackEventArgs : EventArgs
    {
        /// <summary>
        /// 创建新的进度信息
        /// </summary>
        /// <param name="progress">总进度（*/100）</param>
        /// <param name="logInfo">记录信息</param>
        /// <param name="progressStage">阶段性进度（*/100）</param>
        /// <param name="progressStageDetial">阶段性进度信息</param>
        public TaskPostBackEventArgs(int progress, string logInfo, int progressStage, string progressStageDetial)
        {
            Progress = progress;
            LogInfo = logInfo;
            ProgressStage = progressStage;
            ProgressStageDetial = progressStageDetial;
        }
        /// <summary>
        /// 总进度
        /// </summary>
        public Int32 Progress { get; set; }
        /// <summary>
        /// 记录信息 
        /// </summary>
        public String LogInfo { get; set; }
        /// <summary>
        /// 阶段内进度
        /// </summary>
        public Int32 ProgressStage { get; set; }
        /// <summary>
        /// 阶段进度信息
        /// </summary>
        public String ProgressStageDetial { get; set; }
    }
}
