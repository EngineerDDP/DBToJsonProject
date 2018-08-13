using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models.EventArguments
{
    /// <summary>
    /// 更新导出页面信息的消息参数类（功能弃用）
    /// </summary>
    public class ExportPageInfoEventArgs
    {
        public Boolean VdoSelected { get; set; }
        public Boolean ImgSelected { get; set; }
    }
}
