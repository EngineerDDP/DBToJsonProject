using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DBToJsonProject.Controller.SettingManager
{
    abstract class XmlSettingManager
    {
        /// <summary>
        /// 设置节点
        /// </summary>
        protected class SettingNode
        {
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

        /// <summary>
        /// 设置文件根目录
        /// </summary>
        private static readonly string SettingRootFolder = "settings/";
        /// <summary>
        /// 子设置文件目录
        /// </summary>
        protected virtual string SettingFolder { get => ""; }
        /// <summary>
        /// 子设置文件识别名
        /// </summary>
        protected abstract string SettingFile { get; }
        /// <summary>
        /// 获取设置文件根目录
        /// </summary>
        protected static string SettingRootPath { get => SettingRootFolder; }
        protected XmlSettingManager()
        {
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
        /// 判断目录是否存在
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected Boolean ExistSettingXML(String file)
        {
            return ExistSettingXML("", file);
        }
        /// <summary>
        /// 判断目录是否存在
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        protected Boolean ExistSettingXML(String dir,String file)
        {
            return File.Exists(SettingRootFolder + dir + file);
        }
        /// <summary>
        /// 加载设置文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected SettingNode LoadSettingXML(String file)
        {
            return LoadSettingXML("", file);
        }
        /// <summary>
        /// 创建新的空Xml设置文档
        /// </summary>
        /// <param name="xml"></param>
        private void CreateXmlSetting(ref XmlDocument xml)
        {
            XmlDeclaration declaration = xml.CreateXmlDeclaration("1.0", "utf-8", "no");
            xml.AppendChild(declaration);
        }
        /// <summary>
        /// 载入Xml设置文件
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        protected SettingNode LoadSettingXML(String dir,String file)
        {
            FileStream fs;
            XmlDocument xml = new XmlDocument();
            dir = SettingRootFolder + dir;
            if (File.Exists(dir + file))
            {
                fs = File.Open(dir + file, FileMode.Open);
                xml.Load(fs);
            }
            else
            {
                Directory.CreateDirectory(dir);
                fs = File.Open(dir + file, FileMode.Create);
                CreateXmlSetting(ref xml);
            }
            fs.Close();

            return new SettingNode(xml.DocumentElement);
        }
        protected void SaveSettingXML(String file, SettingNode setting)
        {
            SaveSettingXML("", file, setting);
        }
        /// <summary>
        /// 保存Xml设置
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="file"></param>
        /// <param name="setting"></param>
        protected void SaveSettingXML(String dir, String file, SettingNode setting)
        {
            FileStream fs;
            dir = SettingRootFolder + dir;
            if (File.Exists(dir + file))
            {
                fs = File.Open(dir + file, FileMode.Open);
                fs.SetLength(0);
            }
            else
            {
                Directory.CreateDirectory(dir);
                fs = File.Open(dir + file, FileMode.Create);
            }
            XmlDocument xml = new XmlDocument();
            CreateXmlSetting(ref xml);
            xml.AppendChild(setting.ToXmlElement(xml));
            xml.Save(fs);

            fs.Close();
        }
        #region 抽象方法
        protected abstract void Load();
        protected abstract void Init();
        public abstract void Update();
        #endregion
    }
}
