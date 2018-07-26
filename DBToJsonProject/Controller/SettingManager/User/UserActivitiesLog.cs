using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DBToJsonProject.Controller.SettingManager
{
    class UserActivitiesLog
    {
        private List<KeyValuePair<DateTime,String>> logs;
        public UserActivitiesLog()
        {
            logs = new List<KeyValuePair<DateTime, string>>();
        }
        /// <summary>
        /// 记录Log信息，并返回格式化后的Log字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public String Log(String msg)
        {
            DateTime t = DateTime.Parse(DateTime.Now.ToString());
            logs.Add(new KeyValuePair<DateTime, string>(t, msg));
            return String.Format("{0} Info:{1}", t.ToShortTimeString(), msg);
        }
        /// <summary>
        /// 序列化Log到流
        /// </summary>
        /// <param name="stream"></param>
        public void Serialize(StreamWriter stream)
        {
            foreach(KeyValuePair<DateTime,String> d in logs)
            {
                String log = String.Format("{0} Info:{1}", d.Key, d.Value);
                stream.WriteLine(log);
            }
            stream.Flush();
        }
    }
}
