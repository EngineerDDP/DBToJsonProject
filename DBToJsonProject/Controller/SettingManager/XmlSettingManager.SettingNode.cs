using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DBToJsonProject.Controller.SettingManager
{
    abstract partial class XmlSettingManager
    {
        /// <summary>
        /// 设置节点
        /// </summary>
        protected class SettingNode
        {
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
            public String Name
            {
                get; set;
            }
            public Dictionary<String, String> Attributes
            {
                get; set;
            }
            public List<SettingNode> ChildNodes
            {
                get; set;
            }
            public SettingNode(String name)
            {
                Name = name;
                Attributes = new Dictionary<string, string>();
                ChildNodes = new List<SettingNode>();
            }
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
            public void SetAttribute(String name, String value)
            {
                if (Attributes.Keys.Contains(name))
                    Attributes[name] = value;
                else
                    Attributes.Add(name, value);
            }
            public void AppendChild(SettingNode node)
            {
                ChildNodes.Add(node);
            }
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
