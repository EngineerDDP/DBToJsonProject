using System;
using System.Collections.Generic;
using DBToJsonProject.Models;
using DBToJsonProject.Models.EventArguments;

namespace DBToJsonProject.Views.WorkSpace
{
    public interface IWorkPage
    {
        /// <summary>
        /// 生成取消操作的消息
        /// </summary>
        event EventHandler CancelExcution;
        /// <summary>
        /// 生成执行操作的消息
        /// </summary>
        event EventHandler<CmdExecuteArgs> ExecuteCmd;
        /// <summary>
        /// 更新用户选择
        /// </summary>
        event EventHandler<SelectCollection> SelectionUpdated;

        /// <summary>
        /// 初始化选项集合
        /// </summary>
        /// <param name="s">选项集合参数</param>
        void SetupSelections(SelectCollection s);
        /// <summary>
        /// 更新任务进度
        /// </summary>
        /// <param name="args">任务进度参数</param>
        void TaskPostBack(TaskPostBackEventArgs args);
        /// <summary>
        /// 更新文件清单
        /// </summary>
        /// <param name="files">文件清单列表</param>
        void UpdateFileList(List<FileExpression> files);
        /// <summary>
        /// 更新页面信息
        /// </summary>
        /// <param name="args">页面信息参数</param>
        void UpdatePageInfos(ExportPageInfoEventArgs args);
    }
}