using System;
using System.Collections.Generic;
using System.Text;

namespace DBToJsonProject.Controller.SettingManager
{
    public class TreeNode : IJsonTreeNode
    {
        public TreeNode(string jsonNodeName, string dbName, string displayName, bool multiRelated, bool buildSingleFile, bool selectable, bool virtualNode)
        {
            JsonNodeName = jsonNodeName;
            DbName = dbName;
            DisplayName = displayName;
            MultiRelated = multiRelated;
            BuildSingleFile = buildSingleFile;
            Selectable = selectable;
            VirtualNode = virtualNode;
        }

        /// <summary>
        /// 生成的Json文件中该节点属性名
        /// </summary>
        public String JsonNodeName { get; set; }
        /// <summary>
        /// 虚结点，不写入Json
        /// </summary>
        public Boolean VirtualNode { get; set; }
        /// <summary>
        /// 数据库中该节点对应实体名
        /// </summary>
        public String DbName { get; set; }
        /// <summary>
        /// 相对用户，该节点显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 该节点是否有多个实体，指示该节点在生成时，是否生成为Json数组 如[1,2,3,4]
        /// </summary>
        public bool MultiRelated { get; set; }
        /// <summary>
        /// 该节点是否建立Json文件，如果比该节点高的节点此值也设置为True，则两个文件中均包含本节点内容
        /// </summary>
        public bool BuildSingleFile { get; set; }
        /// <summary>
        /// 是否为可选值
        /// </summary>
        public bool Selectable { get; set; }
        /// <summary>
        /// 父节点
        /// </summary>
        public IJsonTreeNode Parent { get; set; }
        /// <summary>
        /// 子节点组
        /// </summary>
        public Dictionary<String, IJsonTreeNode> ChildNodes { get; set; }
        /// <summary>
        /// 自定义查询
        /// </summary>
        public ICustomizedSqlDescriber Sql { get; set; }

        public bool Equals(IJsonTreeNode obj)
        {
            if (this.JsonNodeName == obj.JsonNodeName)
                return Parent == null ? true : Parent.Equals(obj);
            else
                return false;
        }
    }
}
