﻿using DBToJsonProject.Controller.SettingManager;
using System;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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

        public SqlCommandCache(IJsonTreeNode treeNode, String specifiedQuaryStringsArgs)
        {
            paras = new ParameterCache[0];
            sqlTemplate = String.Format(treeNode.Sql.CustomizeSQLString, specifiedQuaryStringsArgs);
        }
        /// <summary>
        /// 建立SQL查询指令缓存
        /// </summary>
        /// <param name="currentNode">当前节点的IJson抽象索引</param>
        /// <param name="parentNode">当前节点的父节点IJson抽象索引</param>
        /// <param name="specifiedQuaryStringsArgs">系统级全局参数字符串</param>
        public SqlCommandCache(IJsonTreeNode currentNode, IJsonTreeNode parentNode, String[] specifiedQuaryStringsArgs)
        {
            int i = 0, j = 0;
            bool isallstring = true;
            ICustomizedSqlDescriber describer = currentNode.Sql;
            String dbname = currentNode.DbName;

            var tmpParas = new List<ParameterCache>();

            foreach(Parameter p in describer.Params.Parameters)
            {
                tmpParas.Add(new ParameterCache(p, parentNode));
                isallstring &= p.IsString;
                i++;
            }

            //处理SQL
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
            i = Regex.Matches(sqlTemplate, @"{\d}").Count;
            try
            {
                //处理可选项
                if (currentNode.HasSelectionNode)
                    foreach (IJsonTreeNode n in currentNode.ChildNodes.Values)
                    {
                        string str;
                        if (n.IsSelectionNode && BuildSelection(n, out str))
                        {
                            if (n.Sql.HasCustomizeSQLString)
                                str = "(" + string.Format(n.Sql.CustomizeSQLString, str) + ")";
                            sqlTemplate += String.Format(" AND {0} IN {1}", n.DbName, str);
                        }
                        else if (n.IsSelectionNode)
                        {
                            sqlTemplate += String.Format(" AND 0 = 1");
                            break;
                        }
                    }
            }
            catch(Exception)
            {
                throw new Exception("处理可选项时发生错误,SQL Template：" + sqlTemplate);
            }
            //处理参数列表
            try
            {
                while (i != tmpParas.Count)
                {
                    tmpParas.Add(new ParameterCache(specifiedQuaryStringsArgs[j++]));
                    if (j > specifiedQuaryStringsArgs.Length)
                        j = 0;
                }
            }
            catch (Exception)
            {
                throw new Exception("处理参数列表发生错误,SQL Template：" + sqlTemplate);
            }
            paras = tmpParas.ToArray();
        }
        /// <summary>
        /// 根据选项创建筛选集合
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool BuildSelection(IJsonTreeNode node, out string result)
        {
            string r = string.Empty;
            foreach(IJsonTreeNode n in node.ChildNodes.Values)
            {
                if (n.IsSelected)
                {
                    r += n.DbName + ",";
                }
            }
            if (String.IsNullOrEmpty(r))            //无选项，无筛选结果
                result = r;
            else
                result = string.Format("({0})", r.Substring(0, r.Length - 1));    //去最后一个逗号，创建数组
            return !String.IsNullOrEmpty(r);
        }
        /// <summary>
        /// 使用JsonObject具象化抽象参数，并获取SQL语句实例
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetInstance(JObject obj)
        {
            int i = 0;
            string[] args = new string[paras.Length];

            foreach (ParameterCache para in paras)
            {
                if(!para.GetParam(obj, out args[i]))
                {
                    return String.Empty;
                }
                i++;
            }
            try
            {
                var str = String.Format(sqlTemplate, args);
                return str;
            }
            catch(Exception)
            {
                throw new Exception("动态初始化发生错误，Sql Template:" + sqlTemplate);
            }
        }
    }
}
