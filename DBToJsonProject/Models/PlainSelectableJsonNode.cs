using System;
using System.Collections.Generic;

namespace DBToJsonProject.Models
{
    /// <summary>
    /// 用于在后端传送的类，记录用户上次的选项
    /// </summary>
    public class PlainSelectableJsonNode
    {
        /// <summary>
        /// 防止名称重复而使用的自增标签
        /// </summary>
        private static uint index = 0x01c98f2;
        /// <summary>
        /// 自增标签
        /// </summary>
        public string Tag
        {
            get
            {
                index += 1;
                return string.Format("a{0:X8}", index);
            }
        }
        /// <summary>
        /// 选项名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 子选项
        /// </summary>
        public List<PlainSelectableJsonNode> Childs
        {
            get;set;
        }
        /// <summary>
        /// 使用特定的选项名称构建选项
        /// </summary>
        /// <param name="name">选项名称</param>
        public PlainSelectableJsonNode(string name)
        {
            Name = name;
            Childs = new List<PlainSelectableJsonNode>();
        }
        public PlainSelectableJsonNode(SelectableJsonNode node) : this(node.Name) { }
        public PlainSelectableJsonNode(SelectableJsonList select) : this(select.Name)
        {
            foreach (SelectableJsonNode node in select.Nodes)
                Childs.Add(new PlainSelectableJsonNode(node));
        }
        /// <summary>
        /// 提供和前端模型相互转换的方法
        /// </summary>
        /// <param name="list"></param>
        public static implicit operator PlainSelectableJsonNode(SelectableJsonList list)
        {
            return new PlainSelectableJsonNode(list);
        }
        /// <summary>
        /// 提供和前端模型相互转换的方法
        /// </summary>
        /// <param name="node"></param>
        public static implicit operator PlainSelectableJsonNode(SelectableJsonNode node)
        {
            return new PlainSelectableJsonNode(node);
        }
    }
}
