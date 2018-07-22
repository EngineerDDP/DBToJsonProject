using System;

namespace DBToJsonProject.Controller.SettingManager
{
    public interface ICustomizedSqlDescriber
    {
        String CustomizeSQLString { get; }
        ICustomizedSqlParameters Params { get; }
    }
}
