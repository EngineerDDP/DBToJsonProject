using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBToJsonProject.TaskManager
{
    class DataBaseAccess
    {
        public DataBaseAccess(String dbConnectStr)
        {

        }
        /// <summary>
        /// 匹配一个数据库中的指定项
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dbRow">数据库行的一部分</param>
        /// <returns></returns>
        public bool MatchRow(String tableName, Dictionary<String,object> dbRow)
        {
            return true;
        }
        /// <summary>
        /// 依照指定的列名称匹配并填充字典
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <param name="dbColumnNames">数据库表的列名</param>
        /// <returns></returns>
        public List<Dictionary<String,object>> FillDictionary(String tableName, List<String> dbColumnNames)
        {
            List<Dictionary<String, object>> resultList = new List<Dictionary<string, object>>();       //创建空结果集
            Random r = new Random();

            int rCount = r.Next(0, 100);
            for (int i = 0; i < rCount; ++i)        //依照对应关系，将数据库中每一行中元素映射到目标Json标签下
            {
                Dictionary<String, object> dataRow = new Dictionary<string, object>();
                int value = r.Next(0x00, 0x7fffffff);
                foreach (String key in dbColumnNames)
                {
                    dataRow.Add(key, String.Format("{0:X000}", value));
                }
                resultList.Add(dataRow);
            }
            return resultList;
        }
        /// <summary>
        /// 将数据库行写入数据库
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dbRow"></param>
        /// <returns></returns>
        public int WriteToDB(String tableName,Dictionary<String,object> dbRow)
        {
            return dbRow.Count;
        }
    }
}
