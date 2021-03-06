﻿using DBToJsonProject.Controller.SettingManager;
using DBToJsonProject.TaskManager;
using System;
using System.Collections.Generic;

namespace DBToJsonProject.Controller
{
    class UserValidation
    {
        public String Username { get; private set; }
        public String Password { get; private set; }
        public Boolean IsUserValidated { get; private set; }
        public UserValidation(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public bool Validate()
        {
            DBSettings settings = DBSettings.Default;
            Dictionary<String, object> userDbRow = new Dictionary<string, object>()
                {
                { settings.UserRoot.UserName, Username },
                { settings.UserRoot.Password, Password }
                };
            //DataBaseAccess db = new DataBaseAccess(DBSettings.Default.DBConnectStr);
            //IsUserValidated = db.MatchRow(settings.UserRoot.TableName, userDbRow);
            IsUserValidated = true;
            return true;
        }
    }
}
