using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models
{
    public class SelectCollection : EventArgs
    {
        public List<SelectableJsonList> Source { get; set; }
        public SelectCollection()
        {
            Source = new List<SelectableJsonList>();
        }
    }
}
