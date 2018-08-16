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
        private static readonly string Xml_ExportFolder = "ExportFolder";
        private static readonly string Xml_WindowSettingRoot = "Window";
        private static readonly string Xml_WindowLocX = "LocX";
        private static readonly string Xml_WindowLocY = "LocY";
        private static readonly string Xml_WindowWidth = "Width";
        private static readonly string Xml_WindowHeight = "Height";
        public Int32 UpdateDelay
        {
            get => 500;
        }
        public String ActiveUser
        {
            get; set;
        }
        public String ExportWorkFolder
        {
            get;set;
        }
        public Double WindowLeft { get; set; }
        public Double WindowTop { get; set; }
        public Double WindowWidth { get; set; }
        public Double WindowHeight { get; set; }
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
        private AppSetting()
        {
            base.Start();
        }
        protected override void Load()
        {
            SettingNode root = LoadSettingXML(SettingFile);
            ActiveUser = root.Attributes[Xml_ActiveUserNodeName];
            ExportWorkFolder = root.Attributes[Xml_ExportFolder];

            SettingNode window = root.SelectSingleNode(Xml_WindowSettingRoot);
            WindowLeft = Double.Parse(window.Attributes[Xml_WindowLocX]);
            WindowTop = Double.Parse(window.Attributes[Xml_WindowLocY]);
            WindowWidth = Double.Parse(window.Attributes[Xml_WindowWidth]);
            WindowHeight = Double.Parse(window.Attributes[Xml_WindowHeight]);
        }
        protected override void Init()
        {
            ActiveUser = "";
            ExportWorkFolder = "ExportResult/";
            WindowLeft = 200;
            WindowTop = 200;
            WindowWidth = 1062;
            WindowHeight = 677;
        }
        public override void Update()
        {
            SettingNode root = new SettingNode(Xml_SettingRootName);
            root.SetAttribute(Xml_ActiveUserNodeName, ActiveUser);
            root.SetAttribute(Xml_ExportFolder, ExportWorkFolder);

            SettingNode window = new SettingNode(Xml_WindowSettingRoot);
            window.SetAttribute(Xml_WindowLocX, WindowLeft.ToString());
            window.SetAttribute(Xml_WindowLocY, WindowTop.ToString());
            window.SetAttribute(Xml_WindowWidth, WindowWidth.ToString());
            window.SetAttribute(Xml_WindowHeight, WindowHeight.ToString());
            root.AppendChild(window);

            SaveSettingXML(SettingFile, root);
        }
    }
}
