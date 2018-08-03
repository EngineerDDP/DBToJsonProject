using System;
using System.Collections.Generic;
using System.Linq;

namespace DBToJsonProject.Controller.SettingManager
{
    [Serializable]
    public class UnSolvedParametersException : Exception
    {
        public override string Message
        {
            get
            {
                return "无法解析的自定义参数。";
            }
        }
        public String ParaName { get; set; }
        public String Node { get; set; }
    }
    public struct Parameter
    {
        public IJsonTreeNode nvalue;
        public String svalue;
        public bool IsString
        {
            get
            {
                return nvalue == null;
            }
        }
    }
    /// <summary>
    /// 自定义参数标定名称，参数标定格式为"paraA>childB///,paraB/"，其中反斜线数目为父级层次数目，字符串写在最前，代表该表下属性的Json标记名称（注意：不是数据库列名），多个标记之间用逗号隔开
    /// </summary>
    public class CustomizedSqlParameters : ICustomizedSqlParameters
    {
        public List<Parameter> Parameters { get; private set; }
        private string describe;
        public override string ToString()
        {
            return describe;
        }
        public CustomizedSqlParameters()
        {
            Parameters = new List<Parameter>();
            describe = "";
        }
        public CustomizedSqlParameters(String str, IJsonTreeNode current) : this()
        {
            if (!String.IsNullOrWhiteSpace(str))
            {
                describe = str;
                string[] paras = str.Split(',');
                foreach (string argv in paras)
                {
                    int c = argv.Count(q => q == '/');
                    if (c > 0)
                    {
                        IJsonTreeNode n = current;
                        for (int parentlv = c; parentlv != 0 && n != null; parentlv--)
                        {
                            n = n.Parent;
                        }
                        string[] s = argv
                            .Substring(0, argv.Length - c).Split('>')
                            .SkipWhile(q => String.IsNullOrEmpty(q))
                            .ToArray();
                        foreach (string i in s)
                        {
                            if (n == null)
                                break;
                            if (!n.ChildNodes.TryGetValue(i, out n))
                                throw new UnSolvedParametersException()
                                {
                                    ParaName = argv,
                                    Node = current.JsonNodeName
                                };
                        }
                        if (n != null)
                            Parameters.Add(new Parameter() { nvalue = n });
                    }
                    else
                        Parameters.Add(new Parameter() { svalue = argv });
                }
            }
        }
    }
}
