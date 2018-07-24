using System;

namespace DBToJsonProject.Controller.SettingManager
{
    public interface ICustomizedSqlDescriber
    {
        Boolean HasCustomizeSQLString { get; }
        String CustomizeSQLString { get; }
        ICustomizedSqlParameters Params { get; }
    }
}
