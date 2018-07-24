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
        public static readonly string ReadTable = "读数据库表 {0} ...";
        public static readonly string WriteFile = "写文件 {0} ...";
    }
    public class DBSettingErrorException : Exception
    { }
    class ExportTask
    {
        public event EventHandler<TaskPostBackEventArgs> UpdateProgressInfo;

        DataBaseAccess dataBaseAccess;
        SelectableJsonNode[] selections;
        JsonEntityDetial detial;
        String[] SpecifiedQuaryStrings;

        int progressPercentage = 0;
        string progressStage = "";
        string progressStageInfo = "";

        internal DataBaseAccess DataBaseAccess { get => dataBaseAccess; set => dataBaseAccess = value; }
        public SelectableJsonNode[] Selections { get => selections; set => selections = value; }
        public JsonEntityDetial Detial { get => detial; set => detial = value; }

        /// <summary>
        /// 执行导出数据任务的类
        /// </summary>
        /// <param name="dbConStr"></param>
        /// <param name="jsonSelections"></param>
        /// <param name="topNodeSqlStr"></param>
        public ExportTask(String dbConStr, SelectableJsonNode[] jsonSelections, JsonEntityDetial detial)
        {
            DataBaseAccess = new DataBaseAccess(dbConStr);
            Selections = jsonSelections;
            Detial = detial;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            Update(UpdateStrings.Initialize, 0, UpdateStrings.Initialize);
            DataBaseAccess.DBColumnDosentExist += DataBaseAccess_DBColumnDosentExist;
            Task t = new Task(Execution);
            t.Start();
        }

        private void DataBaseAccess_DBColumnDosentExist(object sender, DBColumnDosentExistEvent e)
        {
            Update(String.Format(UpdateStrings.DBColumnsNotFound, e.TableName, String.Concat(e.ColumnNames.Select(q => q + ", "))),
                null );
        }
        private void Update(String loginfo, int? progress)
        {
            Update(loginfo, progress, progressStageInfo);
        }
        private void Update(String loginfo, int? progress, String progressdetial)
        {
            UpdateProgressInfo?.Invoke(this, new TaskPostBackEventArgs()
            {
                LogInfo = loginfo,
                Progress = (progress == null ? progressPercentage : progress.Value),
                ProgressStage = progressStage,
                ProgressStageDetial = progressdetial
            });
        }
        private void Execution()
        {
            BuildJsonFiles();
        }
        /// <summary>
        /// 填充JsonObject
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="sqlcmd"></param>
        /// <returns></returns>
        private JObject FillJsonObject(IJsonTreeNode node, string sqlcmd)
        {
            Update(String.Format(UpdateStrings.ReadTable, node.DbName), null);

            JObject obj = new JObject();
            Dictionary<String, String> dbToJson = GetColumnNamesFunc(node);
            if (dbToJson.Count != 0)
            {
                List<Dictionary<String, String>> datarows = DataBaseAccess.FillDictionary(sqlcmd, dbToJson.Keys.ToArray());

                foreach (String key in datarows[0].Keys)
                {
                    obj.Add(dbToJson[key], datarows[0][key]);
                }
            }
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
            Update(String.Format(UpdateStrings.ReadTable, node.DbName), null);

            JArray arr = new JArray();
            Dictionary<String, String> dbToJson = GetColumnNamesFunc(node);
            if (dbToJson.Count != 0)
            {
                List<Dictionary<String, String>> datarows = DataBaseAccess.FillDictionary(sqlcmd, dbToJson.Keys.ToArray());
                foreach (Dictionary<String, String> row in datarows)
                {
                    JObject o = new JObject();
                    foreach (String key in row.Keys)
                    {
                        o.Add(dbToJson[key], row[key]);
                    }
                    arr.Add(o);
                }
            }
            return arr;
        }
        /// <summary>
        /// 建立数据库列名到Json属性名的映射
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private Dictionary<String,String> GetColumnNamesFunc(IJsonTreeNode node)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();
            foreach(IJsonTreeNode child in node.ChildNodes.Values)
            {
                if(child.IsDBColumn)
                {
                    result.Add(child.DbName, child.JsonNodeName);
                }
            }
            return result;
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

            List<String> args = new List<string>();

            foreach (Parameter i in currentNode.Sql.Params.Parameters)
            {
                if (i.IsStatic)
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
                    args.Add((String)v[trace.Pop()]);
                }
                else
                    args.Add(i.svalue);
            }

            return String.Format(sqlcmd, args.ToArray());
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
                string s = SpecifiedQuaryStrings[i];
                object obj = String.Empty;
                if(node.BuildSingleFile)
                {
                    if (node.MultiRelated)
                    {
                        obj = FillJsonArray(node, s);
                        foreach (JObject o in obj as JArray)
                        {
                            progressPercentage = (100 / detial.roots.Count) * c++ / (obj as JArray).Count;
                            foreach (IJsonTreeNode n in node.ChildNodes.Values)
                                o.Add(n.JsonNodeName, BuildJsonNode(n, o, node));
                        }
                    }
                    else
                    {
                        obj = FillJsonObject(node, s);
                        foreach (IJsonTreeNode n in node.ChildNodes.Values)
                            (obj as JObject).Add(n.JsonNodeName, BuildJsonNode(n, obj as JObject, node));
                    }
                }
                WriteFile(obj, node.JsonNodeName);
                i++;
            }
            Update(UpdateStrings.COmplete, 100, UpdateStrings.COmplete);
        }
        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        private void WriteFile(object obj, String filename)
        {
            using (FileStream fs = File.Create(AppSetting.Default.ExportWorkFolder + filename + ".json"))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(obj.ToString());
                sw.Flush();
            }
            Update(String.Format(UpdateStrings.WriteFile, filename), null);
        }
        private JToken BuildJsonNode(IJsonTreeNode node, JObject parentObj, IJsonTreeNode parentNode)
        {
            JToken result;
            bool? check = Selections.FirstOrDefault(q => q.Node.Equals(node))?.IsChecked;
            if (node.MultiRelated)      //多元关系，生成数组,节点结构 "property":[A,B,C,D,E]
            {
                JArray arr;
                if (node.HasVirtualNode)        //存在多选项
                {
                    var j = node.ChildNodes.Values.TakeWhile(q => (Selections.FirstOrDefault(p => p.Node.Equals(q)) != null));      //列出选项
                    arr = new JArray();
                    foreach (IJsonTreeNode i in j)
                    {
                        arr.Concat(FillJsonArray(node, BuildSqlString(parentObj, i, parentNode)));      //针对每个选项构造查找字符串，并用查找结果数组填充本节点
                    }
                }
                else
                {
                    arr = FillJsonArray(node, BuildSqlString(parentObj, node, parentNode));
                }
                foreach (JObject o in arr)
                {
                    foreach (IJsonTreeNode k in node.ChildNodes.Values)
                        o.Add(k.JsonNodeName, BuildJsonNode(k, o, node));
                }
                result = arr;
            }
            else        //单元关系，生成对象
            {
                JObject obj = FillJsonObject(node, BuildSqlString(parentObj, node, parentNode));
                foreach (IJsonTreeNode k in node.ChildNodes.Values)
                    obj.Add(k.JsonNodeName, BuildJsonNode(k, obj, node));

                result = obj;
            }
            if (node.BuildSingleFile)
                WriteFile(result, node.JsonNodeName);
            return result;
        }
    }
}
