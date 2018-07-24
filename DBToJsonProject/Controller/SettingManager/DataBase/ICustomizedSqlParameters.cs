using System.Collections.Generic;

namespace DBToJsonProject.Controller.SettingManager
{
    public interface ICustomizedSqlParameters
    {
        List<Parameter> Parameters { get; }
        string ToString();
    }
}