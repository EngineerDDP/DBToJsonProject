using DBToJsonProject.Models;
using System;
using System.IO;
using System.Linq;

namespace DBToJsonProject.Controller.SettingManager
{
    class UserSetting : XmlSettingManager
    {
        private readonly string UserFolder;
        protected override string SettingFolder => UserFolder;
        protected override string SettingFile => "profile.xml";

        private readonly string LogFileName = "activities.log";

        private readonly string Xml_BaseSettingRoot = "A";
        private readonly string Xml_PasswordAttr = "C";
        private readonly string Xml_AutoLogAttr = "D";
        private readonly string Xml_ExportSettingNode = "G";
        

        private string name;
        private String savedpass;
        private bool autoLogin;
        private UserActivitiesLog log;
        private PlainSelectableJsonNode Sel;
        public string Name { get => name; set => name = value; }
        public string Savedpass { get => savedpass; set => savedpass = value; }
        public bool AutoLogin { get => autoLogin; set => autoLogin = value; }

        public UserSetting(string username)
        {
            Name = username;
            UserFolder = String.Format("{0}/", Name);
            log = new UserActivitiesLog();
            Sel = new PlainSelectableJsonNode(Xml_ExportSettingNode);

            if(ExistSettingXML(SettingFolder,SettingFile))
            {
                Load();
            }
            else
            {
                Init();
            }
        }
        /// <summary>
        /// 载入
        /// </summary>
        protected override void Load()
        {
            //获取根元素
            SettingNode root = LoadSettingXML(UserFolder, SettingFile);
            AutoLogin = Boolean.Parse(root.Attributes[Xml_AutoLogAttr]);
            Savedpass = root.Attributes[Xml_PasswordAttr];

            //读上次保存的选项集合
            root = root.SelectSingleNode(Sel.Name);
            foreach (SettingNode s in root.ChildNodes)
            {
                PlainSelectableJsonNode list = new PlainSelectableJsonNode(s.Value);
                foreach (SettingNode n in s.ChildNodes)
                {
                    list.Childs.Add(new PlainSelectableJsonNode(n.Value));
                }
                Sel.Childs.Add(list);
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            SettingNode root = new SettingNode(Xml_BaseSettingRoot);
            root.SetAttribute(Xml_AutoLogAttr, AutoLogin.ToString());
            root.SetAttribute(Xml_PasswordAttr, Savedpass);

            SettingNode sel = new SettingNode(Sel.Name);
            //写选项集合
            foreach (PlainSelectableJsonNode i in Sel.Childs)
            {
                SettingNode n = new SettingNode(i.Tag) { Value = i.Name };
                foreach (PlainSelectableJsonNode j in i.Childs)
                {
                    n.AppendChild(new SettingNode(j.Tag) { Value = j.Name });
                }
                sel.AppendChild(n);
            }
            root.AppendChild(sel);
            SaveSettingXML(UserFolder, SettingFile, root);

            //写入Log文件
            using (FileStream fs = File.Create(SettingRootPath + UserFolder + LogFileName))
            {
                log.Serialize(new StreamWriter(fs));
            }
        }
        /// <summary>
        /// 写上次记录的用户设置
        /// </summary>
        /// <param name="selections"></param>
        public void LoadSelections(ref SelectCollection selections)
        {
            foreach(PlainSelectableJsonNode l in Sel.Childs)
            {
                foreach(PlainSelectableJsonNode n in l.Childs)
                {
                    (selections.Source.FirstOrDefault(q => q.Name == l.Name)?.
                        Nodes.FirstOrDefault(q => q.Name == n.Name)).IsChecked = true;
                }
            }
        }
        /// <summary>
        /// 记录用户选择
        /// </summary>
        /// <param name="selections"></param>
        public void SaveSelections(SelectCollection selections)
        {
            Sel.Childs = new System.Collections.Generic.List<PlainSelectableJsonNode>();
            foreach(SelectableJsonList list in selections.Source)
            {
                PlainSelectableJsonNode l = new PlainSelectableJsonNode(list.Name);
                foreach(SelectableJsonNode node in list.Nodes)
                {
                    if (node.IsChecked)
                        l.Childs.Add(node);
                }
                Sel.Childs.Add(l);
            }
        }
        /// <summary>
        /// 初始化新对象
        /// </summary>
        protected override void Init()
        {
            AutoLogin = false;
        }
        public void RememberedPass(String password)
        {
            Savedpass = password;
        }
        public void ForgetPass()
        {
            Savedpass = "";
        }
        public void SetAutoLogin()
        {
            AutoLogin = true;
        }
        public string PostLog(string msg)
        {
            return log.Log(msg);
        }
    }
}
