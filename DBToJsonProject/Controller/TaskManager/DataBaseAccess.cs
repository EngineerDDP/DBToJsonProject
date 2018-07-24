using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            sqlCon.ConnectionString = connectStr;
        }
        private void SomeDBWorks()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(200);
            });
        }
        /// <summary>
        /// 匹配一个数据库中的指定项
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dbRow">数据库行的一部分</param>
        /// <returns></returns>
        public bool MatchRow(String tableName, Dictionary<String,string> dbRow)
        {
            sqlCon.Open();
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
            sqlCon.Close();

            return result;
        }
        /// <summary>
        /// 查询并填充字典
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <param name="dbColumnNames">数据库表的列名</param>
        /// <exception cref="DBColumnDosentExistException">异常，如果存在列名找不到</exception>
        /// <returns></returns>
        public List<Dictionary<String, String>> FillDictionary(String sqlCommand, string[] sampleColumns)
        {
            List<Dictionary<String, String>> result = new List<Dictionary<string, string>>();
            List<String> crruptedColumName = new List<string>();

            sqlCon.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = sqlCon;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sqlCommand;

            SqlDataReader reader = cmd.ExecuteReader();
            
            while(reader.Read())
            {
                Dictionary<String, String> row = new Dictionary<string, string>();
                foreach(String key in sampleColumns)
                {
                    if (!crruptedColumName.Contains(key))
                    {
                        try
                        {
                            row.Add(key, reader.GetString(reader.GetOrdinal(key)));
                        }
                        catch (IndexOutOfRangeException)
                        {
                            crruptedColumName.Add(key);
                        }
                    }
                }
                result.Add(row);
            }

            reader.Close();
            sqlCon.Close();

            if (crruptedColumName.Count != 0)
                DBColumnDosentExist?.Invoke(this, new DBColumnDosentExistEvent()
                {
                    ColumnNames = crruptedColumName.ToArray()
                });

            return result;
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
            sqlCon.Open();
            sqlBulkCopy.WriteToServer(dt);
            sqlCon.Close();
        }
    }
}
