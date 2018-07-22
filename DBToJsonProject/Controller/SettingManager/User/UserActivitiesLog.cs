using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller.SettingManager
{
    class UserActivitiesLog
    {
        private Dictionary<DateTime, String> logs;
        public UserActivitiesLog()
        {
            logs = new Dictionary<DateTime, string>();
        }
        /// <summary>
        /// 记录Log信息，并返回格式化后的Log字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public String Log(String msg)
        {
            DateTime t = DateTime.Parse(DateTime.Now.ToString());
            logs.Add(t, msg);
            return String.Format("{0} Info:{1}", t.ToShortTimeString(), msg);
        }
        /// <summary>
        /// 序列化Log到流
        /// </summary>
        /// <param name="stream"></param>
        public void Serialize(StreamWriter stream)
        {
            foreach(DateTime d in logs.Keys)
            {
                String log = String.Format("{0} Info:{1}", d.ToString(), logs[d]);
                stream.WriteLine(log);
            }
            stream.Flush();
        }
    }
}
