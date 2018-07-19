using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DBToJsonProject.SettingManager
{
    public interface JsonTreeNode
    {
        String JsonNodeName { get;  }
        String DbName { get;  }
        List<JsonTreeNode> ChildNodes { get; }
    }
    public interface UserTableInfo
    {
        String TableName { get; }
        String UserName { get; }
        String Password { get; }
    }
    class DBSettings
    {
        private readonly static String ExportRootNodeName = "ExportTree";
        private readonly static String ImportRootNodeName = "ImportTree";
        private readonly static String UserTableNodeName = "UserTable";
        private readonly static String UserNameAttrition = "UserName";
        private readonly static String PasswordAttrition = "Password";
        private readonly static String RootNodeName = "Setting";
        private readonly static String DbTableAttributeName = "DbTableName";
        private readonly static String DbColumnAttributeName = "DbColumnName";
        private readonly static String DbConnectionString = "DbConStr";

        /// <summary>
        /// 构建Json节点与数据库节点对应关系图，每个JsonObject对应一个数据库表名，每个Json节点下面可能包含元素标签，也可能嵌套其他JsonObject
        /// </summary>
        class TreeNode : JsonTreeNode
        {
            public String JsonNodeName { get; set; }
            public String DbName { get; set; }
            public List<JsonTreeNode> ChildNodes { get; set; }
        }
        class UserInfo : UserTableInfo
        {
            public String TableName { get; set; }
            public String UserName { get; set; }
            public String Password { get; set; }
        }
        /// <summary>
        /// 使用单例模式维护设置
        /// </summary>
        private static DBSettings instance;

        private List<JsonTreeNode> exportRoot;
        private List<JsonTreeNode> importRoot;
        private UserInfo userRoot;
        private String DBConnectArgs;
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
                    exportRoot = BuildTreeNodeFromXml(node).ChildNodes;         //为导出Json建立配置文件

                    node = xml.SelectSingleNode(ImportRootNodeName);
                    importRoot = BuildTreeNodeFromXml(node).ChildNodes;         //为导入Json建立配置文件

                    node = xml.SelectSingleNode(UserTableNodeName);             
                    userRoot = new UserInfo()
                    {
                        TableName = node.Attributes[DbColumnAttributeName].Value,
                        UserName = node.Attributes[UserNameAttrition].Value,
                        Password = node.Attributes[PasswordAttrition].Value
                    };           //为验证用户信息配置数据库

                    DBConnectArgs = xml.SelectSingleNode(RootNodeName).Attributes[DbConnectionString].Value;
                }
            }
        }
        private TreeNode BuildTreeNodeFromXml(XmlNode xmlNode)
        {
            List<JsonTreeNode> childs = new List<JsonTreeNode>();
            foreach (XmlNode n in xmlNode.ChildNodes)
            {
                childs.Add(BuildTreeNodeFromXml(n));
            }

            var node = new TreeNode()
            {
                JsonNodeName = xmlNode.Value,
                ChildNodes = childs
            };
            if (childs.Count == 0)
                node.DbName = xmlNode.Attributes[DbColumnAttributeName].Value;
            else
                node.DbName = xmlNode.Attributes[DbTableAttributeName].Value;
            return node;
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
        /// <summary>
        /// 获取默认设置对象
        /// </summary>
        public static DBSettings Default
        {
            get
            {
                if (instance == null)
                    instance = new DBSettings();
                return instance;
            }
        }

        public List<JsonTreeNode> ExportRoot { get => exportRoot; }
        public List<JsonTreeNode> ImportRoot { get => importRoot; }
        public UserTableInfo UserRoot { get => userRoot; }
        public string DBConnectStr { get => DBConnectArgs; }
    }
}
