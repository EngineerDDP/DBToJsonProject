using DBToJsonProject.Controller.SettingManager;
using DBToJsonProject.Models;
using DBToJsonProject.Models.EventArguments;
using DBToJsonProject.TaskManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace DBToJsonProject.Controller.TaskManager
{
    
    public class UpdateStrings
    {
        public static readonly string DBColumnsNotFound = "数据库表 {0} 中，找不到名为 {1} 的列.";
        public static readonly string[] ProgressStage = new string[] {
            "构造Json文件主体结构",
            "读数据库表",
            "写文件",
            "任务完成"
        };
        public static readonly string COmplete = "导出完成";
        public static readonly string Initialize = "初始化资源...";
        public static string ReadTable(string name)
        {
            return String.Format("读数据库表 {0} ...", name);
        }
        public static string WriteFile(string name)
        {
            return String.Format("更新文件 {0}", name);
        }
        public static string FillObj(string name)
        {
            return String.Format("查找资源 {0}", name);
        }
        public static string BuildObj(string name)
        {
            return String.Format("构造文件 \"{0}\" ", name);
        }
        public static readonly string Canceled = "任务被取消";
        public static readonly string Working = "工作进行中...";
    }
    public class DBSettingErrorException : Exception
    { }
    class ExportTask : ITask, IDisposable
    {
        public event EventHandler<StringEventArgs> PostErrorAndAbort;           //报告错误并退出运行
        public event EventHandler<TaskPostBackEventArgs> UpdateProgressInfo;    //更新执行信息

        DataBaseAccess dataBaseAccess;              //数据库连接
        SelectableJsonNode[] selections;            //选项集合
        JsonEntityDetial detial;                    //导出信息
        String[] SpecifiedQuaryStringsArgs;         //系统参数
        Task workingThread;                         //执行线程
        Task ReportThread;

        bool CancelProcess = false;         //取消线程
        int totalProgress = 0;              //进度
        int stageProgress = 0;              //局部进度
        string progressStage = "";          //当前进度
        string loginfo = "";
        public int Progress
        {
            get
            {
                return totalProgress;
            }
        }
        public bool Complete
        {
            get
            {
                return totalProgress == 100;
            }
        }

        internal DataBaseAccess DataBaseAccess { get => dataBaseAccess; set => dataBaseAccess = value; }
        public SelectableJsonNode[] Selections { get => selections; set => selections = value; }
        public JsonEntityDetial Detial { get => detial; set => detial = value; }

        /// <summary>
        /// 执行导出数据任务的类
        /// </summary>
        /// <param name="dbConStr"></param>
        /// <param name="jsonSelections"></param>
        /// <param name="topNodeSqlStr"></param>
        public ExportTask(String dbConStr, SelectableJsonNode[] jsonSelections, JsonEntityDetial detial, String[] args)
        {
            DataBaseAccess = new DataBaseAccess(dbConStr);
            Selections = jsonSelections;
            Detial = detial;
            SpecifiedQuaryStringsArgs = args;
        }
        /// <summary>
        /// 初始化环境并启动工作线程
        /// </summary>
        public void Run()
        {
            progressStage = UpdateStrings.Initialize;
            stageProgress = 0;
            totalProgress = 0;
            Update(UpdateStrings.Initialize);
            DataBaseAccess.DBColumnDosentExist += DataBaseAccess_DBColumnDosentExist;

            ReportThread = new Task(KeepUpdate);
            ReportThread.Start();
            
            workingThread = new Task(Execution);
            workingThread.Start();
        }
        public void Cancel()
        {
            CancelProcess = true;
        }
        private void DataBaseAccess_DBColumnDosentExist(object sender, DBColumnDosentExistEvent e)
        {
            Update(String.Format(UpdateStrings.DBColumnsNotFound, e.TableName, String.Concat(e.ColumnNames.Select(q => q + ", "))));
        }
        private void KeepUpdate()
        {
            while(!CancelProcess && totalProgress != 100)
            {
                if (!String.IsNullOrEmpty(loginfo))
                {
                    Update(loginfo);
                    loginfo = String.Empty;
                }
                Thread.Sleep(1000);
            }
        }
        private void Update(String loginfo)
        {
            UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs(
            totalProgress, loginfo, stageProgress, progressStage));
        }
        private void Execution()
        {
            try
            {
                dataBaseAccess.OpenConnection();
                BuildJsonFilesAsync().Wait();
            }
            catch (AggregateException e)
            {
                totalProgress = 100;
                PostErrorAndAbort?.Invoke(this, new StringEventArgs()
                {
                    Str = "信息:" + e.InnerException.GetType().ToString() + e.InnerException.Message
                });
            }
            dataBaseAccess.CloseConnection();
        }
        /// <summary>
        /// 填充JsonObject
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="sqlcmd"></param>
        /// <returns></returns>
        private async Task<JObject> FillJsonObjectAsync(IJsonTreeNode node, string sqlcmd)
        {
            JArray arr;
            JObject obj;
            String[] sample;
            String[] target;

            GetColumnNamesFunc(node, out sample, out target);
            arr = await DataBaseAccess.FillDictionaryAsync(sqlcmd, sample, target);

            if (arr.Count >= 1)
                obj = arr[0] as JObject;
            else
                obj = new JObject();
            
            return obj;
        }
        /// <summary>
        /// 填充Json数组
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="sqlcmd"></param>
        /// <returns></returns>
        private async Task<JArray> FillJsonArrayAsync(IJsonTreeNode node, string sqlcmd)
        {
            JArray arr;
            String[] sample;
            String[] target;

            GetColumnNamesFunc(node, out sample, out target);
            arr = await DataBaseAccess.FillDictionaryAsync(sqlcmd, sample, target);
            
            return arr;
        }
        private Dictionary<IJsonTreeNode, string[][]> columnNameCache = new Dictionary<IJsonTreeNode, string[][]>();
        /// <summary>
        /// 建立数据库列名到Json属性名的映射
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private void GetColumnNamesFunc(IJsonTreeNode node, out string[] sample, out string[] target)
        {
            string[][] cache;
            if (!columnNameCache.TryGetValue(node, out cache))
            {
                List<string> smp = new List<string>();
                List<string> tgt = new List<string>();

                foreach (IJsonTreeNode child in node.ChildNodes.Values)
                {
                    if (child.IsDBColumn)
                    {
                        smp.Add(child.DbName);
                        tgt.Add(child.JsonNodeName);
                    }
                }

                sample = smp.ToArray();
                target = tgt.ToArray();

                columnNameCache.Add(node, new string[][] { sample, target });
            }
            else
            {
                sample = cache[0];
                target = cache[1];
            }
        }
        /// <summary>
        /// 节点参数速查缓存
        /// </summary>
        private Dictionary<IJsonTreeNode, SqlCommandCache> parameterCache = new Dictionary<IJsonTreeNode, SqlCommandCache>();
        /// <summary>
        /// 使用回溯搜索参数列表中的参数，并取出对应的值
        /// </summary>
        /// <param name="parentObject"></param>
        /// <param name="currentNode"></param>
        /// <param name="parentNode"></param>
        /// <param name="extendedStr"></param>
        /// <returns></returns>
        private string BuildSqlString(JObject parentObject, IJsonTreeNode currentNode, IJsonTreeNode parentNode)
        {
            SqlCommandCache sql;
            if(!parameterCache.TryGetValue(currentNode,out sql))
            {
                sql = new SqlCommandCache(currentNode.Sql, parentNode, currentNode.DbName);
                parameterCache.Add(currentNode, sql);
            }
            if (currentNode.Sql.Params.Parameters.Count == 0)               //无参数
                return sql.GetInstance(SpecifiedQuaryStringsArgs);          //填充系统参数
            else
                return sql.GetInstance(parentObject);                       //填充动态参数
        }
        /// <summary>
        /// 根据设置构建Json文件
        /// </summary>
        private async Task BuildJsonFilesAsync()
        {
            int i = 0;
            foreach(IJsonTreeNode node in detial.roots)
            {
                int c = 0;
                string s = String.Format(node.Sql.CustomizeSQLString, SpecifiedQuaryStringsArgs);
                object obj = String.Empty;
                bool buildstate = false;

                progressStage = UpdateStrings.Working;

                if(node.BuildSingleFile)
                {
                    progressStage = UpdateStrings.BuildObj(node.JsonNodeName);
                    if (node.MultiRelated)
                    {
                        obj = await FillJsonArrayAsync(node, s);
                        foreach (JObject o in obj as JArray)
                        {
                            totalProgress = i * (100 / detial.roots.Count) + (stageProgress / detial.roots.Count);
                            stageProgress = 100 * c++ / (obj as JArray).Count;

                            buildstate = await buildChildsAsync(node, o);
                            if (!buildstate)
                                break;
                        }
                    }
                    else
                    {
                        obj = await FillJsonObjectAsync(node, s);
                        buildstate = await buildChildsAsync(node, obj as JObject);
                    }
                }
                if(buildstate)
                    WriteFile(obj, node.JsonNodeName);
                i++;
            }

            progressStage = UpdateStrings.COmplete;
            totalProgress = 100;
            stageProgress = 100;
            Update(UpdateStrings.COmplete);
        }
        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        private void WriteFile(object obj, String filename)
        {
            if (!Directory.Exists(AppSetting.Default.ExportWorkFolder))
                Directory.CreateDirectory(AppSetting.Default.ExportWorkFolder);
            using (FileStream fs = File.Create(AppSetting.Default.ExportWorkFolder + filename + ".json"))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(obj.ToString());
                sw.Flush();
            }
            Update(UpdateStrings.WriteFile(filename));
        }
        /// <summary>
        /// 为数组构建子结构
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="currentObj"></param>
        private async Task<bool> buildChildsAsync(IJsonTreeNode currentNode, JArray currentObj)
        {
            foreach (JObject o in currentObj)
            {
                if (!await buildChildsAsync(currentNode, o))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 为对象构建子结构
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="currentObj"></param>
        private async Task<bool> buildChildsAsync(IJsonTreeNode currentNode, JObject currentObj)
        {
            bool build = false;
            bool EndNode = true;
            
            foreach (IJsonTreeNode k in currentNode.ChildNodes.Values)
            {
                if (k.IsDbTable)
                {
                    JToken t = await BuildJsonNodeAsync(k, currentObj, currentNode);
                    currentObj.Add(k.JsonNodeName, t);
                    build = true;
                    EndNode = false;
                }
            }
            return build || EndNode;
        }
        /// <summary>
        /// 构建子结构
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentObj"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        private async Task<JToken> BuildJsonNodeAsync(IJsonTreeNode node, JObject parentObj, IJsonTreeNode parentNode)
        {
            JToken result = new JObject();
            bool? check = Selections.FirstOrDefault(q => q.Node.Equals(node))?.IsChecked;       //查找本节点选择情况
            if (check == false || parentObj == null)
                return result;

            if (CancelProcess)          //检查进程退出标志
                throw new Exception(UpdateStrings.Canceled);

            loginfo = UpdateStrings.ReadTable(node.DbName);
            
            if (node.MultiRelated)      //多元关系，生成数组,节点结构 "property":[A,B,C,D,E]
            {
                JArray arr;
                if (node.HasSelectionNode)        //存在多选项
                {
                    var j = Selections.Where(q => q.Node.Parent == node && q.IsChecked).Select(q => q.Node);      //列出选项
                    if (j.Count() == 0)         //未选任意类别
                        return result;
                    arr = new JArray();
                    foreach (IJsonTreeNode i in j)
                    {
                        JToken t = await FillJsonArrayAsync(node, BuildSqlString(parentObj, i, parentNode));
                        arr = new JArray(arr.Concat(t));
                    }
                }
                else
                {
                    arr = await FillJsonArrayAsync(node, BuildSqlString(parentObj, node, parentNode));
                }
                if (await buildChildsAsync(node, arr))
                    result = arr;
            }
            else        //单元关系，生成对象
            {
                JObject obj = await FillJsonObjectAsync(node, BuildSqlString(parentObj, node, parentNode));
                if (await buildChildsAsync(node, obj))
                    result = obj;
            }
            if (node.BuildSingleFile)
                WriteFile(result, node.JsonNodeName);
            return result;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    dataBaseAccess.Dispose();
                    workingThread.Dispose();
                    ReportThread.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~ExportTask() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
