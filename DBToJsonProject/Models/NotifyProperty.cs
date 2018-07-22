using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models
{
    public class NotifyProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void UpdatePropertyChange(string paraName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paraName));
        }
    }
}
