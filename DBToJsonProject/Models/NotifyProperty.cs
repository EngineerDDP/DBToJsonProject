using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models
{
    /// <summary>
    /// 实现自动更新接口，并提供简单的更新方法
    /// </summary>
    public class NotifyProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 生成参数变更事件，通知所有关联的对象
        /// </summary>
        /// <param name="paraName"></param>
        protected void UpdatePropertyChange(string paraName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paraName));
        }
    }
}
