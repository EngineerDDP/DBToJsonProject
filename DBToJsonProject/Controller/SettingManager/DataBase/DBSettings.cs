using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller.SettingManager
{
    public class JsonEntityDetial
    {
        public List<IJsonTreeNode> roots { get; set; }
        public String DbConnectStr { get; set; }
        public JsonEntityDetial()
        {
            roots = new List<IJsonTreeNode>();
        }
    }

    public class UserInfo : IUserTableInfo
    {
        public String DbConnectStr { get; set; }
        public String TableName { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }
    }

    class DBSettings : XmlSettingManager
    {
        protected override String SettingFolder => "dbsettings/";
        protected override string SettingFile => "setting.xml";
        /// <summary>
        /// 导出Tree根节点名
        /// </summary>
        private readonly static String ExportRootNodeName = "ExportTree";
        /// <summary>
        /// 导入Tree根节点名
        /// </summary>
        private readonly static String ImportRootNodeName = "ImportTree";
        /// <summary>
        /// 用户表名
        /// </summary>
        private readonly static String UserTableName = "UserTable";
        /// <summary>
        /// 用户节点名
        /// </summary>
        private readonly static String UserNodeName = "UserTable";
        /// <summary>
        /// 用户名列名
        /// </summary>
        private readonly static String UserNameAttrition = "UserName";
        /// <summary>
        /// 密码列名
        /// </summary>
        private readonly static String PasswordAttrition = "Password";
        /// <summary>
        /// 设置根节点名
        /// </summary>
        private readonly static String RootNodeName = "Setting";
        /// <summary>
        /// 数据库实体名
        /// </summary>
        private readonly static String DbEntityAttributeName = "DbColumnName";
        /// <summary>
        /// 对用户的显示名称
        /// </summary>
        private readonly static String DbDisplayName = "DbDisplayName";
        /// <summary>
        /// 该节点是否生成独立Json文件
        /// </summary>
        private readonly static String BuildJsonFile = "BuildJsonFile";
        /// <summary>
        /// 数据库关联类型标签
        /// </summary>
        private readonly static String DbTableMultiRelated = "DbTableMultiRelated";
        /// <summary>
        /// 数据库连接字符串标签
        /// </summary>
        private readonly static String DbBaseTableConnectString = "DbBaseTableConnectString";
        /// <summary>
        /// 自定义Sql查询语句
        /// </summary>
        private readonly static String DbCustomizedSql = "DbCustomizedSql";
        /// <summary>
        /// 该节点是否为可选项
        /// </summary>
        private readonly static String NodeSelectable = "NodeSelectable";
        /// <summary>
        /// 
        /// </summary>
        private readonly static String IsVirtual = "IsVirtual";
        /// <summary>
        /// 自定义Sql语句中的参数绑定
        /// </summary>
        private readonly static String DbCustomizedSqlParameters = "DbCustomizedSqlParameters";
        /// <summary>
        /// 构建Json节点与数据库节点对应关系图，每个JsonObject对应一个数据库表名，每个Json节点下面可能包含元素标签，也可能嵌套其他JsonObject
        /// </summary>

        /// <summary>
        /// 使用单例模式维护设置
        /// </summary>
        private static DBSettings instance;

        private JsonEntityDetial exportEntities;
        private JsonEntityDetial importEntities;
        private IUserTableInfo userRoot;
        private String DBConnectArgs;
        /// <summary>
        /// 初始化数据库配置文件
        /// </summary>
        private DBSettings()
        {
            base.Start();
        }
        private delegate void X(out JsonEntityDetial entityDetial, ref SettingNode n);
        protected override void Load()
        {
            //获取XML跟元素
            SettingNode root = LoadSettingXML(SettingFolder, SettingFile);
            //读数据库属性
            DBConnectArgs = root.Attributes[DbBaseTableConnectString];


            X readEntities = (out JsonEntityDetial entityDetial, ref SettingNode ro) =>
            {
                entityDetial = new JsonEntityDetial()
                {
                    roots = new List<IJsonTreeNode>()
                };
                //遍历每个Export实体
                foreach (SettingNode n in ro.ChildNodes)
                    entityDetial.roots.Add(BuildTreeNode(n, null));
                //读Export数据库信息
                entityDetial.DbConnectStr = ro.Attributes[DbBaseTableConnectString];
            };

            SettingNode node = root.SelectSingleNode(ExportRootNodeName);
            readEntities(out exportEntities, ref node);
            node = root.SelectSingleNode(ImportRootNodeName);
            readEntities(out importEntities, ref node);

            //读User节点
            node = root.SelectSingleNode(UserNodeName);
            //读User属性
            userRoot = new UserInfo()
            {
                DbConnectStr = node.Attributes[DbBaseTableConnectString],
                TableName = node.Attributes[UserTableName],
                UserName = node.Attributes[UserNameAttrition],
                Password = node.Attributes[PasswordAttrition]
            };
        }
        
        /// <summary>
        /// 序列化新设置到设置文件
        /// </summary>
        public void UpdateSetting(IUserTableInfo userTableInfo, JsonEntityDetial ex, JsonEntityDetial im)
        {
            exportEntities = ex;
            importEntities = im;
            userRoot = userTableInfo;

            Update();
        }
        public override void Update()
        {
            SettingNode root = new SettingNode(RootNodeName);
            
            root.SetAttribute(DbBaseTableConnectString, DBConnectArgs);

            SettingNode exportTree = new SettingNode(ExportRootNodeName);
            exportTree.SetAttribute(DbBaseTableConnectString, exportEntities.DbConnectStr);

            SettingNode importTree = new SettingNode(ImportRootNodeName);
            importTree.SetAttribute(DbBaseTableConnectString, importEntities.DbConnectStr);

            SettingNode userTable = new SettingNode(UserNodeName);
            userTable.SetAttribute(DbBaseTableConnectString, userRoot.DbConnectStr);
            userTable.SetAttribute(UserNameAttrition, userRoot.UserName);
            userTable.SetAttribute(PasswordAttrition, userRoot.Password);
            userTable.SetAttribute(UserTableName, userRoot.TableName);


            foreach (IJsonTreeNode n in exportEntities.roots)
                exportTree.AppendChild(BuildXmlFromTreeNode(n));

            foreach (IJsonTreeNode n in importEntities.roots)
                importTree.AppendChild(BuildXmlFromTreeNode(n));

            root.AppendChild(exportTree);
            root.AppendChild(importTree);
            root.AppendChild(userTable);

            SaveSettingXML(SettingFolder, SettingFile, root);
        }
        /// <summary>
        /// 从Xml设置文件建立数据库配置项
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private TreeNode BuildTreeNode(SettingNode node, IJsonTreeNode parent)
        {
            //创建对象
            var t = new TreeNode(
                node.Name, 
                node.Attributes[DbEntityAttributeName], 
                node.Attributes[DbDisplayName],
                parent,
                Boolean.Parse(node.Attributes[DbTableMultiRelated]),
                Boolean.Parse(node.Attributes[BuildJsonFile]),
                Boolean.Parse(node.Attributes[NodeSelectable]),
                Boolean.Parse(node.Attributes[IsVirtual])
                );
            //读子节点
            Dictionary<String, IJsonTreeNode> childs = new Dictionary<string, IJsonTreeNode>();
            foreach (SettingNode n in node.ChildNodes)
            {
                TreeNode tn = BuildTreeNode(n, t);
                t.ChildNodes.Add(tn.JsonNodeName, tn);
            }
            //读属性
            t.Sql = new CustomizedSqlDescriber(
                node.Attributes[DbCustomizedSql] != String.Empty,
                node.Attributes[DbCustomizedSql],
                node.Attributes[DbCustomizedSqlParameters],
                t);

            return t;
        }
        /// <summary>
        /// 从配置设置构建Xml
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        private SettingNode BuildXmlFromTreeNode(IJsonTreeNode root)
        {
            //创建对象
            SettingNode xml = new SettingNode(root.JsonNodeName);
            //写属性
            xml.SetAttribute(DbEntityAttributeName, root.DbName);
            xml.SetAttribute(DbTableMultiRelated, root.MultiRelated.ToString());
            xml.SetAttribute(DbDisplayName, root.DisplayName);
            xml.SetAttribute(BuildJsonFile, root.BuildSingleFile.ToString());
            xml.SetAttribute(NodeSelectable, root.Selectable.ToString());
            xml.SetAttribute(DbCustomizedSql, root.Sql.CustomizeSQLString);
            xml.SetAttribute(DbCustomizedSqlParameters, root.Sql.Params.ToString());
            xml.SetAttribute(IsVirtual, root.VirtualNode.ToString());

            //先写实节点
            foreach (String key in root.ChildNodes.Keys)
            {
                if(root.ChildNodes[key].IsDBColumn)
                    xml.AppendChild(BuildXmlFromTreeNode(root.ChildNodes[key]));
            }
            //虚结点做右子节点
            foreach (String key in root.ChildNodes.Keys)
            {
                if (!root.ChildNodes[key].IsDBColumn)
                    xml.AppendChild(BuildXmlFromTreeNode(root.ChildNodes[key]));
            }
            return xml;
        }
        protected override void Init()
        {
            userRoot = new UserInfo()
            {
                DbConnectStr = "",
                UserName = "username",
                Password = "password",
                TableName = ""
            };
            importEntities = new JsonEntityDetial();
            exportEntities = new JsonEntityDetial();

            Update();
        }
        /// <summary>
        /// 创建与前端交互的Selection对象
        /// </summary>
        public Models.SelectCollection BuildSelections()
        {
            Models.SelectCollection selections = new Models.SelectCollection();
            foreach (IJsonTreeNode root in ExportRoot.roots)
            {
                Models.SelectableJsonList list = new Models.SelectableJsonList(root.DisplayName, root);

                //使用宽度优先搜索遴选出可选节点并标记
                Queue<IJsonTreeNode> que = new Queue<IJsonTreeNode>();
                que.Enqueue(root);
                while (que.Count != 0)
                {
                    //BFS
                    IJsonTreeNode n = que.Dequeue();
                    foreach (IJsonTreeNode i in n.ChildNodes.Values)
                        que.Enqueue(i);
                    //筛选节点
                    if (n.Selectable)
                        list.Nodes.Add(new Models.SelectableJsonNode(n.DisplayName, n));
                }
                if(list.Nodes.Count != 0)
                    selections.Source.Add(list);
            }
            return selections;
        }
        public void SetupBuildFiles(Models.SelectCollection selections)
        {
            foreach(Models.SelectableJsonList l in selections.Source)
            {
                bool toBuild = false;
                foreach(Models.SelectableJsonNode n in l.Nodes)
                {
                    toBuild |= n.IsChecked;
                }
                (l.Node as TreeNode).BuildSingleFile  = toBuild;
            }
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


        /// <summary>
        /// 获取每个导出文件对应的JsonObject关系树，每个子元素对应一个文件
        /// </summary>
        public JsonEntityDetial ExportRoot { get => exportEntities; set => exportEntities = value; }
        /// <summary>
        /// 获取每个导入文件对应的JsonObject关系树，每个子元素对应一个文件
        /// </summary>
        public JsonEntityDetial ImportRoot { get => importEntities; set => importEntities = value; }
        /// <summary>
        /// 获取用户相关配置项，包括用户表名称，连接数据库使用的字符串，用户名和密码的列名
        /// </summary>
        public IUserTableInfo UserRoot { get => userRoot; }
        /// <summary>
        /// 缺省数据库连接字符串
        /// </summary>
        public string DBConnectStr { get => DBConnectArgs; }
    }
}
