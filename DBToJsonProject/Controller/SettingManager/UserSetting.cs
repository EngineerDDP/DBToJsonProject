using DBToJsonProject.Models;
using System;
using System.IO;
using System.Xml;

namespace DBToJsonProject.Controller.SettingManager
{
    class UserSetting : XmlSettingManager
    {
        private readonly string UserFolder;
        private readonly string BaseSettingFileName = "profile.xml";
        private readonly string LogFileName = "activities.log";

        private readonly string Xml_BaseSettingRoot = "A";
        private readonly string Xml_PasswordAttr = "C";
        private readonly string Xml_AutoLogAttr = "D";
        private readonly string Xml_ExportSettingNode = "G";
        

        private string name;
        private String savedpass;
        private bool autoLogin;
        private UserActivitiesLog log;
        private SelectableJsonList userSelections;

        public UserSetting(string username)
        {
            name = username;
            UserFolder = String.Format("{0}/", name);
            log = new UserActivitiesLog();

            if(base.ExistSettingXML(UserFolder,BaseSettingFileName))
            {
                Load();
            }
            else
            {
                Create();
            }
        }
        private void Load()
        {
            XmlDocument xml = new XmlDocument();
            using (FileStream file = base.LoadSettingXML(UserFolder, BaseSettingFileName))
            {
                xml.Load(file);
            }
            //获取根元素
            XmlElement root = xml.DocumentElement;
            autoLogin = Boolean.Parse(root.Attributes[Xml_AutoLogAttr].Value);
            savedpass = root.Attributes[Xml_PasswordAttr].Value;
            XmlNode node = root.SelectSingleNode(Xml_ExportSettingNode);
            foreach(XmlNode n in node.ChildNodes)
            {
                if(n.NodeType == XmlNodeType.Element)
                {
                    
                    foreach(XmlNode i in n.ChildNodes)
                    {
                        
                    }
                }
            }
        }
        public override void Update()
        {
            using (FileStream file = base.LoadSettingXML(UserFolder, BaseSettingFileName))
            {

            }
        }
        private void Create()
        {

        }
        public void RememberedPass(String password)
        {
            savedpass = password;
        }
        public void SetAutoLogin()
        {
            autoLogin = true;
        }
        public string PostLog(string msg)
        {
            return log.Log(msg);
        }
    }
}
