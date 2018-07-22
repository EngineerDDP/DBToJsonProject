using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models
{
    public class Selections : EventArgs
    {
        public List<SelectableJsonList> Source { get; set; }
        public Selections()
        {
            Source = new List<SelectableJsonList>();
        }
    }
}
