using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DBToJsonProject.SettingManager
{
    class DBSettings
    {
        private readonly static String ExportRootNodeName = "ExportTree";
        private readonly static String ImportRootNodeName = "ImportTree";
        private readonly static String UserTableNodeName = "UserTable";
        private readonly static String RootNodeName = "Setting";
        private readonly static String DbTableAttributeName = "DbTableName";
        private readonly static String DbColumnAttributeName = "DbColumnName";

        /// <summary>
        /// 构建Json节点与数据库节点对应关系图，每个JsonObject对应一个数据库表名，每个Json节点下面可能包含元素标签，也可能嵌套其他JsonObject
        /// </summary>
        private class TreeNode
        {
            public String JsonNodeName { get; set; }
            public String DbName { get; set; }
            public List<TreeNode> ChildNodes { get; set; }
        }
        /// <summary>
        /// 使用单例模式维护设置
        /// </summary>
        private DBSettings instance;

        private TreeNode exportRoot;
        private TreeNode importRoot;
        private TreeNode userRoot;
        /// <summary>
        /// 初始化数据库配置文件
        /// </summary>
        private DBSettings()
        {
            XmlDocument xml = new XmlDocument();
            using (FileStream file = LoadSettingXML())
            {
                if(file.Length <= 2)
                {
                    xml = CreateSetting();
                    xml.Save(file);
                }
                else
                {
                    xml.Load(file);
                    XmlNode node = xml.SelectSingleNode(ExportRootNodeName);
                    exportRoot = new TreeNode()
                    {
                        JsonNodeName = "",
                        DbName = "",
                        ChildNodes = BuildTreeNodeFromXml(node).ChildNodes
                    };
                }
            }
        }
        private TreeNode BuildTreeNodeFromXml(XmlNode xmlNode)
        {
            List<TreeNode> childs = new List<TreeNode>();
            foreach (XmlNode n in xmlNode.ChildNodes)
            {
                childs.Add(BuildTreeNodeFromXml(n));
            }

            var node = new TreeNode()
            {
                JsonNodeName = xmlNode.Name,
                ChildNodes = childs
            };
            if (childs.Count == 0)
                node.DbName = xmlNode.Attributes[DbColumnAttributeName].Value;
            else
                node.DbName = xmlNode.Attributes[DbTableAttributeName].Value;
            return node;
        }
        public DBSettings Default
        {
            get
            {
                if (instance == null)
                    instance = new DBSettings();
                return instance;
            }
        }
        private FileStream LoadSettingXML()
        {
            FileStream fs;
            if(Directory.Exists("setting"))
            {
                fs = File.Open("setting/dbsetting.xml", FileMode.OpenOrCreate);
            }
            else
            {
                Directory.CreateDirectory("setting");
                fs = File.Open("setting/dbsetting.xml", FileMode.Create);
            }
            return fs;
        }
        private XmlDocument CreateSetting()
        {
            XmlDocument xml = new XmlDocument();
            XmlDeclaration declaration = xml.CreateXmlDeclaration("1.0", "utf-8", "no");
            xml.AppendChild(declaration);
            XmlElement root = xml.CreateElement(RootNodeName);
            xml.AppendChild(root);
            XmlNode setting = xml.SelectSingleNode(RootNodeName);
            XmlElement exportTree = xml.CreateElement(ExportRootNodeName);
            XmlElement importTree = xml.CreateElement(ImportRootNodeName);
            XmlElement userTable = xml.CreateElement(UserTableNodeName);
            setting.AppendChild(exportTree);
            setting.AppendChild(importTree);
            setting.AppendChild(userTable);
            return xml;
        }
    }
}
