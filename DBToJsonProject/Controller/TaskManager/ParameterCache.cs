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
                IJsonTreeNode tracker = param.ref_node;
                while (tracker != parent)
                {
                    trace.Push(tracker.JsonNodeName);
                    tracker = tracker.Parent;
                }
                ChildRoute = trace.ToArray();
            }
        }
        /// <summary>
        /// 构造查询条件数组，一般用作 WHERE a IN (...)，本方法即构造括号内的内容（包括括号本身）
        /// </summary>
        /// <param name="array">包含目标数据的Json array</param>
        /// <param name="paraName">需要从Json array中获取的属性名</param>
        /// <param name="str">输出查询集合</param>
        /// <returns>返回是否成功</returns>
        private bool ConcatStringParas(JArray array, String paraName, out string str)
        {
            str = "";
            String result = " ";
            foreach (JObject i in array)
            {
                if (String.IsNullOrWhiteSpace((string)i[paraName]))
                    return false;
                result += (string)i[paraName] + ",";
            }
            str = "(" + result.Substring(0, result.Length - 1) + ")";

            return true;
        }
        /// <summary>
        /// 使用Json对象获取目标查询字符串
        /// </summary>
        /// <param name="jobj">当前活动的Json对象</param>
        /// <param name="str">输出目标查询字符串</param>
        /// <returns>返回是否创建成功的标志</returns>
        public bool GetParam(JObject jobj, out String str)
        {
            str = String.Empty;
            if (IsString)
            {
                str = ChildRoute[0];
            }
            else
            {
                JToken token = jobj;
                int i = 0;

                for (i = 0; i < ChildRoute.Length - 1; ++i)
                {
                    token = token[ChildRoute[i]];
                    if (token.Count() == 0)
                        return false;
                }

                if (token.Type == JTokenType.Array)             //目标是数组，取所有值
                {
                    return ConcatStringParas(token as JArray, ChildRoute[i], out str);
                }
                else
                {
                    str = String.Format("({0})", (string)token[ChildRoute[i]]);
                }
            }
            return true;
        }
    }
}
