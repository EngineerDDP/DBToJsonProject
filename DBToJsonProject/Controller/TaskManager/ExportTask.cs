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
    class ExportTask : ITask
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
            while(!CancelProcess || totalProgress != 100)
            {
                Thread.Sleep(1000);
                if (!String.IsNullOrEmpty(loginfo))
                {
                    Update(loginfo);
                    loginfo = String.Empty;
                }
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
                BuildJsonFiles();
                dataBaseAccess.CloseConnection();
            }
            catch (Exception e)
            {
                totalProgress = 100;
                PostErrorAndAbort?.Invoke(this, new StringEventArgs()
                {
                    Str = "信息:" + e.Message
                });
            }
        }
        /// <summary>
        /// 填充JsonObject
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="sqlcmd"></param>
        /// <returns></returns>
        private JObject FillJsonObject(IJsonTreeNode node, string sqlcmd)
        {
            JArray arr;
            JObject obj = null;
            String[] sample;
            String[] target;

            GetColumnNamesFunc(node, out sample, out target);
            DataBaseAccess.FillDictionary(sqlcmd, sample, target, out arr);

            if (arr.Count >= 1)
                obj = arr[0] as JObject;
            
            return obj;
        }
        /// <summary>
        /// 填充Json数组
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="sqlcmd"></param>
        /// <returns></returns>
        private JArray FillJsonArray(IJsonTreeNode node, string sqlcmd)
        {
            JArray arr;
            String[] sample;
            String[] target;

            GetColumnNamesFunc(node, out sample, out target);
            DataBaseAccess.FillDictionary(sqlcmd, sample, target, out arr);
            
            return arr;
        }
        /// <summary>
        /// 建立数据库列名到Json属性名的映射
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private void GetColumnNamesFunc(IJsonTreeNode node, out string[] sample, out string[] target)
        {
            int i = 0;
            sample = new string[node.ChildNodes.Count];
            target = new string[node.ChildNodes.Count];
            foreach(IJsonTreeNode child in node.ChildNodes.Values)
            {
                if(child.IsDBColumn)
                {
                    sample[i] = child.DbName;
                    target[i] = child.JsonNodeName;
                    i++;
                }
            }
        }
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
            string sqlcmd = currentNode.Sql.HasCustomizeSQLString ?
                currentNode.Sql.CustomizeSQLString : 
                String.Format("Select * From {0} Where ", currentNode.DbName) + "{0} = {1};";
            String result = String.Format("Select * From {0} Where 1=0", currentNode.DbName);

            List<String> args = new List<string>();

            foreach (Parameter i in currentNode.Sql.Params.Parameters)
            {
                if (!i.IsString)
                {
                    Stack<String> trace = new Stack<string>();          //使用回溯，寻找调用路径
                    IJsonTreeNode tracker = i.nvalue;
                    while (tracker != parentNode)
                    {
                        trace.Push(tracker.JsonNodeName);
                        tracker = tracker.Parent;
                        if (tracker == null)
                            throw new DBSettingErrorException();
                    }
                    JToken v = parentObject;
                    while (trace.Count != 1)
                    {
                        v = v[trace.Pop()];
                        if (v == null)
                            throw new DBSettingErrorException();
                    }
                    if (v.Count() == 0)
                        return result;
                    if (v.Type == JTokenType.Array)             //目标是数组，取所有值
                    {
                        ConcatStringParas(v as JArray, trace.Pop());
                        sqlcmd.Replace("=", "IN");
                    }
                    else
                        args.Add((String)v[trace.Pop()]);
                }
                else
                    args.Add(i.svalue);
            }
            if(args.Count == 0)
                return String.Format(sqlcmd, SpecifiedQuaryStringsArgs);
            else
                return String.Format(sqlcmd, args.ToArray());
        }
        private string ConcatStringParas(JArray array, String paraName)
        {
            String result = String.Empty;
            foreach (JObject i in array)
            {
                result += i[paraName] + ",";
            }
            result = "(" + result.Substring(0, result.Length - 1) + ")";

            return result;
        }
        /// <summary>
        /// 根据设置构建Json文件
        /// </summary>
        private void BuildJsonFiles()
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
                        obj = FillJsonArray(node, s);
                        foreach (JObject o in obj as JArray)
                        {
                            totalProgress = (100 / detial.roots.Count) * c / (obj as JArray).Count;
                            stageProgress = 100 * c++ / (obj as JArray).Count;

                            buildstate = buildChilds(node, o);
                            if (!buildstate)
                                break;
                        }
                    }
                    else
                    {
                        obj = FillJsonObject(node, s);
                        buildstate = buildChilds(node, obj as JObject);
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
        private bool buildChilds(IJsonTreeNode currentNode, JArray currentObj)
        {
            foreach (JObject o in currentObj)
            {
                if (!buildChilds(currentNode, o))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 为对象构建子结构
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="currentObj"></param>
        private bool buildChilds(IJsonTreeNode currentNode, JObject currentObj)
        {
            bool isEndNode = true;
            bool build = false;
            
            foreach (IJsonTreeNode k in currentNode.ChildNodes.Values)
            {
                if (k.IsDbTable)
                {
                    JToken t = BuildJsonNode(k, currentObj, currentNode);
                    if (t != null)
                    {
                        currentObj.Add(k.JsonNodeName, t);
                        build = true;
                    }
                }
                isEndNode &= k.IsDBColumn;
            }
            return isEndNode || build;
        }
        /// <summary>
        /// 构建子结构
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentObj"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        private JToken BuildJsonNode(IJsonTreeNode node, JObject parentObj, IJsonTreeNode parentNode)
        {
            JToken result = null;
            bool? check = Selections.FirstOrDefault(q => q.Node.Equals(node))?.IsChecked;
            if (check == false || parentObj == null)
                return result;
            if (CancelProcess)
                throw new Exception(UpdateStrings.Canceled);

            loginfo = UpdateStrings.ReadTable(node.DbName);
            
            if (node.MultiRelated)      //多元关系，生成数组,节点结构 "property":[A,B,C,D,E]
            {
                JArray arr;
                if (node.HasVirtualNode)        //存在多选项
                {
                    var j = Selections.Where(q => q.Node.Parent == node && q.IsChecked).Select(q => q.Node);      //列出选项
                    if (j.Count() == 0)         //未选任意类别
                        return result;
                    arr = new JArray();
                    foreach (IJsonTreeNode i in j)
                    {
                        arr = new JArray(arr.Concat(FillJsonArray(node, 
                                BuildSqlString(parentObj, i, parentNode)))
                                        .ToArray());      //针对每个选项构造查找字符串，并用查找结果数组填充本节点
                    }
                }
                else
                {
                    arr = FillJsonArray(node, BuildSqlString(parentObj, node, parentNode));
                }
                if (buildChilds(node, arr))
                    result = arr;
            }
            else        //单元关系，生成对象
            {
                JObject obj = FillJsonObject(node, BuildSqlString(parentObj, node, parentNode));
                if (buildChilds(node, obj))
                    result = obj;
            }
            if (node.BuildSingleFile)
                WriteFile(result, node.JsonNodeName);
            return result;
        }
    }
}
