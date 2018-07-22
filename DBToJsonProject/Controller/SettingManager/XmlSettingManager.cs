using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller.SettingManager
{
    abstract class XmlSettingManager
    {
        private static readonly string SettingMainFolder = "settings/";
        protected Boolean ExistSettingXML(String dir,String file)
        {
            return File.Exists(SettingMainFolder + dir + file);
        }
        protected FileStream LoadSettingXML(String dir,String file)
        {
            FileStream fs;
            dir = SettingMainFolder + dir;
            if (Directory.Exists(dir))
            {
                fs = File.Open(dir + file, FileMode.OpenOrCreate);
            }
            else
            {
                Directory.CreateDirectory(dir);
                fs = File.Open(dir + file, FileMode.Create);
            }
            return fs;
        }
        public abstract void Update();
    }
}
