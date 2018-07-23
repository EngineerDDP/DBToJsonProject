using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models.EventArguments
{
    public class ExportPageInfoEventArgs
    {
        public string UserName { get; set; }
        public List<DeviceTag> DeviceList { get; set; }
    }
}
