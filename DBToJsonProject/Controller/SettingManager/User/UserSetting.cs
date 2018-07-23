using DBToJsonProject.Models;
using System;
using System.IO;

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
        private SelectCollection Sel;
        public SelectCollection UserSelections
        {
            set => Sel = value;
        }
        public string Name { get => name; set => name = value; }
        public string Savedpass { get => savedpass; set => savedpass = value; }
        public bool AutoLogin { get => autoLogin; set => autoLogin = value; }

        public UserSetting(string username)
        {
            Name = username;
            UserFolder = String.Format("{0}/", Name);
            log = new UserActivitiesLog();
            Sel = new SelectCollection();

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
            root = root.SelectSingleNode(Xml_ExportSettingNode);
            foreach (SettingNode s in root.ChildNodes)
            {
                SelectableJsonList list = new SelectableJsonList(s.Name);
                foreach (SettingNode n in s.ChildNodes)
                {
                    list.Nodes.Add(new SelectableJsonNode(n.Name.Substring(1), null));
                }
                Sel.Source.Add(list);
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

            SettingNode sel = new SettingNode(Xml_ExportSettingNode);
            //写选项集合
            foreach (SelectableJsonList i in Sel.Source)
            {
                SettingNode n = new SettingNode(i.Name);
                foreach (SelectableJsonNode j in i.Nodes)
                {
                    if(j.IsChecked)
                        n.AppendChild(new SettingNode("a" + j.Name));
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
        /// 这什么辣鸡代码？？？
        /// </summary>
        /// <param name="selections"></param>
        public void LoadSelections(ref SelectCollection selections)
        {
            foreach(SelectableJsonList l in Sel.Source)
            {
                for(int i = 0;i < selections.Source.Count;++i)
                {
                    if(l.Name == selections.Source[i].Name)
                    {
                        foreach(SelectableJsonNode k in l.Nodes)
                        {
                            for(int j = 0;j < selections.Source[i].Nodes.Count;++j)
                            {
                                if (selections.Source[i].Nodes[j].Name == k.Name)
                                    selections.Source[i].Nodes[j].IsChecked = true;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 记录用户选择
        /// </summary>
        /// <param name="selections"></param>
        public void SaveSelections(SelectCollection selections)
        {
            Sel = selections;
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
