using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller
{
    class UserValidation
    {
        public String Username { get; set; }
        public String Password { get; set; }
        public UserValidation()
        {

        }
        public bool Validate()
        {
            Dictionary<String, String> userDbRow = new Dictionary<string, string>()
            {
                { "Username", Username },
                { "Password", Password }
            };

        }
    }
}
