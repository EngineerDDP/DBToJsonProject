using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models
{
    /// <summary>
    /// 用于和前端交互的模型，表示了所有可供用户选择的类别
    /// </summary>
    public class SelectCollection : EventArgs
    {
        public List<SelectableJsonList> Source { get; set; }
        public SelectCollection()
        {
            Source = new List<SelectableJsonList>();
        }
    }
}
