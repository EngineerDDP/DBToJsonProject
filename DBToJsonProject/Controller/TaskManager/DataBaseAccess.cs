using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DBToJsonProject.TaskManager
{
    public class DBColumnDosentExistEvent : EventArgs
    {
        public String TableName { get; set; }
        public String[] ColumnNames { get; set; }
    }

    class DataBaseAccess
    {
        private SqlConnection sqlCon;
        public event EventHandler<DBColumnDosentExistEvent> DBColumnDosentExist;
        public DataBaseAccess(String connectStr)
        {
            sqlCon = new SqlConnection();
            sqlCon.ConnectionString = connectStr;
        }
        private void SomeDBWorks()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(200);
            });
        }
        public void OpenConnection()
        {
            sqlCon.Open();
        }
        public void CloseConnection()
        {
            sqlCon.Close();
        }
        /// <summary>
        /// 匹配一个数据库中的指定项
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dbRow">数据库行的一部分</param>
        /// <returns></returns>
        public bool MatchRow(String tableName, Dictionary<String,string> dbRow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = sqlCon;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("Select * From {0} Where ", tableName);

            foreach(string key in dbRow.Keys)
            {
                cmd.CommandText += String.Format("{0} = '{1}' AND ", key, dbRow[key]);
            }
            cmd.CommandText += "1 = 1";

            SqlDataReader reader = cmd.ExecuteReader();
            bool result = reader.HasRows;
            reader.Close();

            return result;
        }
        /// <summary>
        /// 查询并填充字典
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <param name="dbColumnNames">数据库表的列名</param>
        /// <exception cref="DBColumnDosentExistException">异常，如果存在列名找不到</exception>
        /// <returns></returns>
        public void FillDictionary(String sqlCommand, string[] sampleColumns, string[] targetColumns, out JArray arr)
        {
            arr = new JArray();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = sqlCon;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sqlCommand;
            List<String> crruptedColumName = new List<string>();

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                JObject obj = new JObject();
                for (int i = 0; i < sampleColumns.Count() && !String.IsNullOrEmpty(sampleColumns[i]); ++i)
                {
                    try
                    {
                        obj.Add(targetColumns[i], reader[sampleColumns[i]].ToString());
                    }
                    catch (IndexOutOfRangeException)
                    {
                        crruptedColumName.Add(sampleColumns[i]);
                        sampleColumns[i] = String.Empty;
                    }
                }
                arr.Add(obj);
            }

            reader.Close();

            if (crruptedColumName.Count != 0)
                DBColumnDosentExist?.Invoke(this, new DBColumnDosentExistEvent()
                {
                    ColumnNames = crruptedColumName.ToArray()
                });
        }
        /// <summary>
        /// 将字典列表写入数据库
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dbRow"></param>
        /// <returns></returns>
        public void WriteToDB(String tableName, ref List<Dictionary<String, String>> targetDic)
        {
            DataTable dt = new DataTable();
            foreach(String key in targetDic[0].Keys)
            {
                dt.Columns.Add(new DataColumn(key));
            }
            foreach(Dictionary<String,String> dic in targetDic)
            {
                DataRow dr = dt.NewRow();
                foreach(String key in dic.Keys)
                {
                    dr[key] = dic[key];
                }
                dt.Rows.Add(dr);
            }

            SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlCon);
            sqlBulkCopy.DestinationTableName = tableName;
            sqlBulkCopy.BatchSize = dt.Rows.Count;
            sqlBulkCopy.WriteToServer(dt);
        }
    }
}
