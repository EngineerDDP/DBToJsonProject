using System.Collections.Generic;

namespace DBToJsonProject.Controller.SettingManager
{
    public interface ICustomizedSqlParameters
    {
        List<IJsonTreeNode> Parameters { get; }

        string ToString();
    }
}