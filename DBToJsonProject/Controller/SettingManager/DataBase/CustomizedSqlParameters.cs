using System;
using System.Collections.Generic;
using System.Linq;

namespace DBToJsonProject.Controller.SettingManager
{
    /// <summary>
    /// 自定义参数标定名称，参数标定格式为"paraA///,paraB/"，其中反斜线数目为父级层次数目，字符串写在最前，代表该表下属性的Json标记名称（注意：不是数据库列名），多个标记之间用逗号隔开
    /// </summary>
    public class CustomizedSqlParameters : ICustomizedSqlParameters
    {
        public List<IJsonTreeNode> Parameters { get; private set; }
        private string describe;
        public override string ToString()
        {
            return describe;
        }
        public CustomizedSqlParameters()
        {
            Parameters = new List<IJsonTreeNode>();
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
                    IJsonTreeNode n = current;
                    for (int parentlv = argv.Count(q => q == '/'); parentlv != 0 && n != null; parentlv--)
                    {
                        n = n.Parent;
                    }
                    if (n != null)
                        Parameters.Add(n.ChildNodes[argv.Split('/').First()]);
                }
            }
        }
    }
}
