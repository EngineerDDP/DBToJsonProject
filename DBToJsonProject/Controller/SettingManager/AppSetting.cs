using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller.SettingManager
{
    class AppSetting : XmlSettingManager
    {
        private static readonly string SettingFileName = "";
    
        private static readonly string Xml_SettingRootName = "";
        private static readonly string Xml_ActiveUserNodeName = "";

        public AppSetting()
        {
            Load();
        }
        private void Load()
        {
            
        }
        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
