using DBToJsonProject.Controller.SettingManager;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DBToJsonProject.Controller.TaskManager
{
    /// <summary>
    /// 缓存参数回溯路径
    /// </summary>
    internal class ParameterCache
    {
        private bool IsString;
        private string[] ChildRoute;
        public ParameterCache(String str)
        {
            IsString = true;
            ChildRoute = new string[1] { str };
        }
        public ParameterCache(Parameter param, IJsonTreeNode parent)
        {
            if (param.IsString)
            {
                IsString = true;
                ChildRoute = new string[1] { param.svalue };
            }
            else
            {
                Stack<String> trace = new Stack<string>();          //使用回溯，寻找调用路径
                IJsonTreeNode tracker = param.nvalue;
                while (tracker != parent)
                {
                    trace.Push(tracker.JsonNodeName);
                    tracker = tracker.Parent;
                    if (tracker == null)
                        throw new DBSettingErrorException();
                }
                ChildRoute = trace.ToArray();
            }
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
        public string GetParam(JObject o)
        {
            if (IsString)
            {
                return ChildRoute[0];
            }
            else
            {
                JToken v = o;
                int i = 0;

                for (i = 0; i < ChildRoute.Length - 1; ++i)
                {
                    v = v[ChildRoute[i]];
                }

                if (v.Count() == 0)
                    return String.Empty;
                if (v.Type == JTokenType.Array)             //目标是数组，取所有值
                {
                    return ConcatStringParas(v as JArray, ChildRoute[i]);
                }
                else
                {
                    return String.Format("({0})", (string)v[ChildRoute[i]]);
                }
            }
        }
    }
}
