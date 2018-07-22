using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models.EventArguments
{
    public class ExportCmdExecuteArgs : EventArgs
    {
        public SelectCollection Selections
        {
            get;set;
        }
        public String SpecifiedQuaryString
        {
            get;set;
        }
    }
}
