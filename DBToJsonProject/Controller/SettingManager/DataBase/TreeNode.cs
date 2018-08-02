using System;
using System.Collections.Generic;
using System.Text;

namespace DBToJsonProject.Controller.SettingManager
{
    internal class TreeNode : IJsonTreeNode
    {
        public TreeNode(string jsonNodeName, string dbName, string displayName, IJsonTreeNode parent, bool multiRelated, bool buildSingleFile, bool selectable, bool virtualNode)
        {
            JsonNodeName = jsonNodeName;
            DbName = dbName;
            DisplayName = displayName;
            Parent = parent;
            MultiRelated = multiRelated;
            BuildSingleFile = buildSingleFile;
            Selectable = selectable;
            IsSelectionParameter = virtualNode;
            ChildNodes = new Dictionary<string, IJsonTreeNode>();
        }

        /// <summary>
        /// 生成的Json文件中该节点属性名
        /// </summary>
        public String JsonNodeName { get; set; }
        /// <summary>
        /// 虚结点，不写入Json
        /// </summary>
        public Boolean IsSelectionParameter { get; set; }
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
        public bool IsDBColumn {
            get
            {
                if (!isDbColumn.HasValue)
                    initExtendedAttribute();
                return isDbColumn.Value;
            }
        }
        public bool IsDbTable
        {
            get
            {
                if (!isDbTable.HasValue)
                    initExtendedAttribute();
                return isDbTable.Value;
            }
        }
        public bool HasSelectionNode
        {
            get
            {
                if (!hasSelectionNode.HasValue)
                    initExtendedAttribute();
                return hasSelectionNode.Value;
            }
        }
        public bool IsSelectionNode
        {
            get
            {
                if (!isSelectionNode.HasValue)
                    initExtendedAttribute();
                return isSelectionNode.Value;
            }
        }

        private void initExtendedAttribute()
        {
            bool r = true;
            hasSelectionNode = false;

            foreach (TreeNode n in this.ChildNodes.Values)
            {
                if (n.IsSelectionNode)
                    hasSelectionNode |= true;
                if (!n.IsSelectionParameter)
                    r = false;
            }
            isDbColumn = r && !this.IsSelectionParameter;
            isSelectionNode = r && (this.ChildNodes.Count != 0);
            isDbTable = !isDbColumn.Value && !IsSelectionParameter;
            IsSelected = true;
        }
        
        private bool? isSelectionNode;
        private bool? isDbColumn;
        private bool? isDbTable;
        private bool? hasSelectionNode;
        public bool IsSelected { get; set; }

        public bool Equals(IJsonTreeNode obj)
        {
            if (this.JsonNodeName == obj.JsonNodeName)
                return Parent == null ? true : Parent.Equals(obj);
            else
                return false;
        }
    }
}
