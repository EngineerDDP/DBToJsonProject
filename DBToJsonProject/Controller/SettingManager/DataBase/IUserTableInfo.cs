using System;

namespace DBToJsonProject.Controller.SettingManager
{
    public interface IUserTableInfo
    {
        String DbConnectStr { get; }
        String TableName { get; }
        String UserName { get; }
        String Password { get; }
    }
}
