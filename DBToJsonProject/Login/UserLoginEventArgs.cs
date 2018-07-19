using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Login
{
    public class UserLoginEventArgs
    {
        public String Username { get; set; }
        public String Password { get; set; }
        public Boolean RememberPassword { get; set; }
        public Boolean AutomaticLogin { get; set; }
    }
}
