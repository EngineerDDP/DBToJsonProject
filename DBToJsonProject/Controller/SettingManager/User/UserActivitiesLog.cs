using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller.SettingManager
{
    class UserActivitiesLog
    {
        private string file;
        public UserActivitiesLog(String filename)
        {
            file = filename;
        }
        /// <summary>
        /// 记录Log信息，并返回格式化后的Log字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public String Log(String msg)
        {
            DateTime t = DateTime.Parse(DateTime.Now.ToString());

            //写入Log文件
            using (FileStream fs = File.Open(file, FileMode.OpenOrCreate))
            {
                fs.Position = 0;
                if (fs.ReadByte() != 0 && fs.ReadByte() != 0)
                {
                    fs.Position = 0;
                    fs.Write(new byte[] { 0, 0 }, 0, 2);
                    fs.SetLength(2);
                    fs.Flush();
                }
                else
                    fs.Position = fs.Length - 1;

                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                String log = String.Format("{0} Info:{1} \r\n", t, msg);
                sw.WriteLine(log);
                sw.Flush();
            }

            return String.Format("{0} Info:{1}", t.ToShortTimeString(), msg);
        }
        public void Update()
        {
            //结束Log文件
            using (FileStream fs = File.Open(file, FileMode.OpenOrCreate))
            {
                fs.Position = 0;
                fs.Write(System.Text.Encoding.UTF8.GetBytes("Log:"), 0, 2);
                fs.Flush();
            }
        }
    }
}
