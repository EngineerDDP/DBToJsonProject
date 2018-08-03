using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models.EventArguments
{
    /// <summary>
    /// 用于传递执行参数的消息参数类
    /// </summary>
    public class CmdExecuteArgs : EventArgs
    {
        /// <summary>
        /// 构造不包含任何信息的事件参数
        /// </summary>
        public CmdExecuteArgs()
        {

        }
        /// <summary>
        /// 执行导出任务所需要的参数
        /// </summary>
        /// <param name="selections">导出类别选项</param>
        /// <param name="specifiedQuaryStringArgs">特别查询参数</param>
        /// <param name="selectImgs">选择图像</param>
        /// <param name="selectVdos">选择视频</param>
        public CmdExecuteArgs(SelectCollection selections, string[] specifiedQuaryStringArgs, bool selectImgs, bool selectVdos)
        {
            Selections = selections;
            SpecifiedQuaryStringArgs = specifiedQuaryStringArgs;
            SelectImgs = selectImgs;
            SelectVdos = selectVdos;
        }

        public SelectCollection Selections
        {
            get;set;
        }
        public String[] SpecifiedQuaryStringArgs
        {
            get;set;
        }
        public Boolean SelectImgs
        {
            get;set;
        }
        public Boolean SelectVdos
        {
            get;set;
        }
    }
}
