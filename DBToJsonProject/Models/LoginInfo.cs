using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBToJsonProject.Models
{
    class LoginInfo : NotifyProperty
    {
        private String msg;
        private Boolean shown;
        public String Message
        {
            get
            {
                return msg;
            }
            set
            {
                msg = value;
                UpdatePropertyChange("Message");
            }
        }
        public Boolean IsShown
        {
            get
            {
                return shown;
            }
            set
            {
                shown = value;
                UpdatePropertyChange("IsShown");
            }
        }
    }
}
