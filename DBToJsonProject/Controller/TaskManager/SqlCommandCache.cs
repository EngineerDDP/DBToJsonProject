using DBToJsonProject.Controller.SettingManager;
using System;
using Newtonsoft.Json.Linq;

namespace DBToJsonProject.Controller.TaskManager
{
    /// <summary>
    /// 缓存Sql查询语句
    /// </summary>
    internal class SqlCommandCache
    {
        private static readonly string Nonsence = String.Empty;
        private ParameterCache[] paras;
        private string sqlTemplate;

        public SqlCommandCache(ICustomizedSqlDescriber describer, IJsonTreeNode parent, String dbname)
        {
            int i = 0;
            bool isallstring = true;
            paras = new ParameterCache[describer.Params.Parameters.Count];
            foreach(Parameter p in describer.Params.Parameters)
            {
                paras[i] = new ParameterCache(p, parent);
                isallstring &= p.IsString;
                i++;
            }

            if (isallstring)
            {
                sqlTemplate = describer.HasCustomizeSQLString ?
                    describer.CustomizeSQLString :
                    String.Format("Select * From {0} Where ", dbname) + "{0} = {1}";
            }
            else
            {
                sqlTemplate = describer.HasCustomizeSQLString ?
                    describer.CustomizeSQLString :
                    String.Format("Select * From {0} Where ", dbname) + "{0} IN {1}";
            }


        }
        public string GetInstance(JObject obj)
        {
            int i = 0;
            string[] args = new string[paras.Length];

            foreach (ParameterCache para in paras)
            {
                args[i] = para.GetParam(obj);
                if(String.IsNullOrEmpty(args[i]))
                {
                    return String.Empty;
                }
                i++;
            }
            return String.Format(sqlTemplate, args);
        }
        public string GetInstance(String[] args)
        {
            return String.Format(sqlTemplate, args);
        }
    }
}
