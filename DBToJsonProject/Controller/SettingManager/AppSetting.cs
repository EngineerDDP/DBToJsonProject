using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DBToJsonProject.Controller.SettingManager
{
    class AppSetting : XmlSettingManager
    {
        protected override string SettingFile => "appconfig.xml";

        private static readonly string Xml_SettingRootName = "Settings";
        private static readonly string Xml_ActiveUserNodeName = "ActiveUser";

        public String ActiveUser
        {
            get; set;
        }
        private static AppSetting obj;
        public static AppSetting Default
        {
            get
            {
                if (obj == null)
                    obj = new AppSetting();
                return obj;
            }
        }
        private AppSetting() : base() { }
        protected override void Load()
        {
            SettingNode root = LoadSettingXML(SettingFile);
            ActiveUser = root.Attributes[Xml_ActiveUserNodeName];
        }
        protected override void Init()
        {
            ActiveUser = "";
        }
        public override void Update()
        {
            SettingNode root = new SettingNode(Xml_SettingRootName);
            root.SetAttribute(Xml_ActiveUserNodeName, ActiveUser);
            SaveSettingXML(SettingFile, root);
        }
    }
}
