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
        public static readonly string Complete = "导出完成";
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
        public static string Progress(int i)
        {
            return String.Format("进度 ({0}/100)", i);
        }
        public static readonly string PostExecute = "导出文件到Pad";
        public static readonly string Canceled = "任务被取消";
        public static readonly string Working = "工作进行中...";
        public static readonly string ready = "准备就绪";
        public static readonly float DbTotalProcessRate = 0.50f;
    }
    [Serializable]
    public class DBSettingErrorException : Exception
    { }
    class ExportTask : ITask, IDisposable
    {
        public event EventHandler<StringEventArgs> PostErrorAndAbort;           //报告错误并退出运行
        public event EventHandler<TaskPostBackEventArgs> UpdateProgressInfo;    //更新执行信息
        public event EventHandler<FileEventArgs> OnFileOperation;

        DataBaseAccess dataBaseAccess;              //数据库连接
        JsonEntityDetial detial;                    //导出信息
        String[] SpecifiedQuaryStringsArgs;         //系统参数
        Task workingThread;                         //执行线程
        Task ReportThread;
        Boolean ProcessVdo;
        Boolean ProcessImg;

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
        public JsonEntityDetial Detial { get => detial; set => detial = value; }

        /// <summary>
        /// 执行导出数据任务的类
        /// </summary>
        /// <param name="dbConStr"></param>
        /// <param name="jsonSelections"></param>
        /// <param name="topNodeSqlStr"></param>
        public ExportTask(String dbConStr, JsonEntityDetial detial, String[] args, bool vdo, bool img)
        {
            DataBaseAccess = new DataBaseAccess(dbConStr);
            Detial = detial;
            SpecifiedQuaryStringsArgs = args;

            ProcessImg = false;
            ProcessVdo = false;
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
        /// <summary>
        /// 取消操作，通知工作线程退出
        /// </summary>
        public void Cancel()
        {
            CancelProcess = true;
        }
        /// <summary>
        /// 列不存在
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataBaseAccess_DBColumnDosentExist(object sender, DBColumnDosentExistEvent e)
        {
            Update(String.Format(UpdateStrings.DBColumnsNotFound, e.TableName, String.Concat(e.ColumnNames.Select(q => q + ", "))));
        }
        /// <summary>
        /// 保持更新
        /// </summary>
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
        /// <summary>
        /// 向UI线程更新工作状态
        /// </summary>
        /// <param name="loginfo"></param>
        private void Update(String loginfo)
        {
            UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs(
            totalProgress, loginfo, stageProgress, progressStage));
        }
        /// <summary>
        /// 工作
        /// </summary>
        private async void Execution()
        {
            try
            {
                dataBaseAccess.OpenConnection();
                await BuildJsonFilesAsync();
                PostExecution();
                progressStage = UpdateStrings.Complete;
                Update(UpdateStrings.Complete);
            }
            catch(DbSqlException e)
            {
                PostErrorAndAbort?.Invoke(this, new StringEventArgs()
                {
                    Str = "信息:" + e.Message + " SQL: " + e.SqlCommand
                });
            }
            catch (AggregateException e)
            {
                PostErrorAndAbort?.Invoke(this, new StringEventArgs()
                {
                    Str = "信息:" + e.InnerException.Message
                });
            }
            catch (Exception e)
            {
                PostErrorAndAbort?.Invoke(this, new StringEventArgs()
                {
                    Str = "信息:" + e.Message
                });
            }
            finally
            {
                stageProgress = 100;
                totalProgress = 100;
                progressStage = UpdateStrings.ready;
                Update(UpdateStrings.ready);
                dataBaseAccess.CloseConnection();
            }
        }
        private void PostExecution()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "java",
                Arguments = "-jar " + "./DataSynchronize_EX.jar " + SpecifiedQuaryStringsArgs[0].Replace('\'', ' ') + " " 
                                    + Environment.CurrentDirectory + "/" + AppSetting.Default.ExportWorkFolder + " "
                                    + ProcessImg.ToString() + " " + ProcessVdo.ToString(),
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            process.Start();
            StreamReader sr = process.StandardOutput;

            progressStage = UpdateStrings.PostExecute;
            Update(UpdateStrings.PostExecute);

            while (process?.HasExited == false)
            {
                string str = sr.ReadLine();
                int i;
                string[] infos = str?.Split(' ');
                if (infos?.Length > 1)
                    if (Int32.TryParse(infos[0], out i))
                    {
                        if (i == -1)
                            throw new Exception(infos[1]);
                        stageProgress = i;
                        totalProgress = (int)((stageProgress + 100) * UpdateStrings.DbTotalProcessRate);
                        loginfo = infos[1];
                    }
                if (CancelProcess)
                {
                    process?.Kill();
                    throw new Exception(UpdateStrings.Canceled);
                }
            }
            process?.Dispose();
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
                sql = new SqlCommandCache(currentNode, parentNode, SpecifiedQuaryStringsArgs);
                parameterCache.Add(currentNode, sql);
            }
            return sql.GetInstance(parentObject);                       //填充参数
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
                            totalProgress = (int)((i * 100 + stageProgress) * UpdateStrings.DbTotalProcessRate / detial.roots.Count);
                            stageProgress = 100 * c++ / (obj as JArray).Count;

                            buildstate = await buildChildsAsync(node, o);
                            if (!buildstate)
                                break;
                        }
                    }
                    else
                    {
                        stageProgress = 0;
                        obj = await FillJsonObjectAsync(node, s);
                        buildstate = await buildChildsAsync(node, obj as JObject);
                        stageProgress = 100;
                    }
                    totalProgress = (int)((i * 100 + stageProgress) * UpdateStrings.DbTotalProcessRate / detial.roots.Count);
                }
                if (buildstate)
                    WriteFile(obj, node.JsonNodeName);
                i++;
            }

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
            OnFileOperation?.Invoke(this, new FileEventArgs(filename, AppSetting.Default.ExportWorkFolder));
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
            JToken result;

            if (CancelProcess)          //检查进程退出标志
                throw new Exception(UpdateStrings.Canceled);

            loginfo = UpdateStrings.ReadTable(node.DbName);
            
            if (node.MultiRelated)      //多元关系，生成数组
            {
                result = new JArray();
                if (node.IsSelected == false || parentObj == null)
                    return result;
                JArray arr = await FillJsonArrayAsync(node, BuildSqlString(parentObj, node, parentNode));
                if (await buildChildsAsync(node, arr))
                    result = arr;
            }
            else        //单元关系，生成对象
            {
                result = new JObject();
                if (node.IsSelected == false || parentObj == null)
                    return result;
                JObject obj;
                obj = await FillJsonObjectAsync(node, BuildSqlString(parentObj, node, parentNode));
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
