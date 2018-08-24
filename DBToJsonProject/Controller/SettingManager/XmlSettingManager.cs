using System;
using System.IO;
using System.Text;
using System.Xml;

namespace DBToJsonProject.Controller.SettingManager
{
    /// <summary>
    /// 基于Xml的配置文件基类
    /// </summary>
    abstract partial class XmlSettingManager
    {
        /// <summary>
        /// 子设置文件目录
        /// </summary>
        protected virtual string SettingFolder { get; }
        /// <summary>
        /// 子设置文件识别名
        /// </summary>
        protected abstract string SettingFile { get; }
#if DEBUG
        protected static string SettingRootPath { get => ProfileRootPath + "settings\\"; }
        protected static string ProfileRootPath { get => BinDirectory; }
#else
        protected static string SettingRootPath { get => ProfileRootPath + "Profiles\\"; }
        protected static string ProfileRootPath { get => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DataSynchronization_MW\\"; }
#endif
        protected static string BinDirectory { get => "./"; }
        protected void Start()
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
            return File.Exists(SettingRootPath + dir + file);
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
            dir = SettingRootPath + dir;
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
            dir = SettingRootPath + dir;
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
        /// <summary>
        /// 从磁盘加载配置项到内存
        /// </summary>
        protected abstract void Load();
        /// <summary>
        /// 在内存中初始化新的配置项
        /// </summary>
        protected abstract void Init();
        /// <summary>
        /// 更新配置项到磁盘中
        /// </summary>
        public abstract void Update();
        #endregion
    }
}
