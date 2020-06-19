using DBToJsonProject.Controller.SettingManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBToJsonProject.Controller.TaskManager
{
    class JsonExportProcessor
    {
        private DataBaseAccess dbAccess;
        private String[] systemParameters;

        public JsonExportProcessor(DataBaseAccess dataBase, JsonEntityDetial jsonEntity, String[] systemArgs)
        {
            dbAccess = dataBase;
            systemParameters = systemArgs;
        }

        public async Task BuildJsonFile(IJsonTreeNode root)
        {
            JToken res;

            // 根据类型创建
            if (root.MultiRelated)
                res = new JArray();
            else
                res = new JObject();

            
        }
    }
}
