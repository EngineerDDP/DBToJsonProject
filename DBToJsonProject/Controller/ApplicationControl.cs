using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBToJsonProject.Login;
using System.Windows;
using DBToJsonProject.WorkSpace;

namespace DBToJsonProject.Controller
{
    class ApplicationControl
    {
        private LoginWindow login;
        private WorkSpace.WorkWindow work;
        UserValidation user;
        public ApplicationControl()
        {

        }

        public LoginWindow Login { get => login; set => login = value; }
        public WorkWindow Work { get => work; set => work = value; }

        public void Startup()
        {
            Login.OnLogin += Login_OnLogin;
        }

        private void Login_OnLogin(object sender, UserLoginEventArgs args)
        {

            throw new NotImplementedException();
        }
    }
}
