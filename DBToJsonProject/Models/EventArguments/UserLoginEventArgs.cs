using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Models
{
    public class UserLoginEventArgs : EventArgs
    {
        public String Username { get; set; }
        public String Password { get; set; }
        public Boolean RememberPassword { get; set; }
        public Boolean AutomaticLogin { get; set; }
    }
}
