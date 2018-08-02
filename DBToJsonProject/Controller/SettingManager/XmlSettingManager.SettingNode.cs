using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DBToJsonProject.Controller.SettingManager
{
    /// <summary>
    /// 基于Xml的配置文件基类
    /// </summary>
    abstract partial class XmlSettingManager
    {
        /// <summary>
        /// 配置项节点
        /// </summary>
        protected class SettingNode
        {
            /// <summary>
            /// 配置项的值
            /// </summary>
            public String Value
            {
                get
                {
                    return Attributes["Value"];
                }
                set
                {
                    if (Attributes.Keys.Contains("Value"))
                        Attributes["Value"] = value;
                    else
                        Attributes.Add("Value", value);
                }
            }
            /// <summary>
            /// 配置项的名称
            /// </summary>
            public String Name
            {
                get; set;
            }
            /// <summary>
            /// 获取配置项的标签集合
            /// </summary>
            public Dictionary<String, String> Attributes
            {
                get; private set;
            }
            /// <summary>
            /// 获取配置项的子节点集合
            /// </summary>
            public List<SettingNode> ChildNodes
            {
                get; private set;
            }
            /// <summary>
            /// 以指定的名称初始化配置项
            /// </summary>
            /// <param name="name"></param>
            public SettingNode(String name)
            {
                Name = name;
                Attributes = new Dictionary<string, string>();
                ChildNodes = new List<SettingNode>();
            }
            /// <summary>
            /// 以指定的Xml元素初始化配置项
            /// </summary>
            /// <param name="node"></param>
            public SettingNode(XmlElement node)
            {
                Name = node.Name;
                //记录子节点
                ChildNodes = new List<SettingNode>();
                foreach (XmlNode i in node.ChildNodes)
                    if (i.NodeType == XmlNodeType.Element)
                        ChildNodes.Add(new SettingNode(i as XmlElement));
                //记录标签
                Attributes = new Dictionary<string, string>();
                foreach (XmlAttribute a in node.Attributes)
                {
                    Attributes.Add(a.Name, a.Value);
                }
            }
            /// <summary>
            /// 转换配置项到Xml元素
            /// </summary>
            /// <param name="doc"></param>
            /// <returns></returns>
            public XmlElement ToXmlElement(XmlDocument doc)
            {
                XmlElement element = doc.CreateElement(Name);
                foreach (String n in Attributes.Keys)
                {
                    element.SetAttribute(n, Attributes[n]);
                }
                foreach (SettingNode s in ChildNodes)
                {
                    element.AppendChild(s.ToXmlElement(doc));
                }
                return element;
            }
            /// <summary>
            /// 添加配置项的参数
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public void SetAttribute(String name, String value)
            {
                if (Attributes.Keys.Contains(name))
                    Attributes[name] = value;
                else
                    Attributes.Add(name, value);
            }
            /// <summary>
            /// 添加配置项的子项
            /// </summary>
            /// <param name="node"></param>
            public void AppendChild(SettingNode node)
            {
                ChildNodes.Add(node);
            }
            /// <summary>
            /// 选择特定名称的子节点
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public SettingNode SelectSingleNode(String name)
            {
                foreach(SettingNode n in ChildNodes)
                {
                    if (n.Name == name)
                        return n;
                }
                return null;
            }
        }
    }
}
