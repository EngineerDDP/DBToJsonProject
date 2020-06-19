using System;
using System.Collections.Generic;
using System.Linq;

namespace DBToJsonProject.Controller.SettingManager
{
    public class CustomizedSqlParametersException : Exception
    {
        public String ParaName { get; set; }
        public String Node { get; set; }
        public String Advice { get; set; }
    }
    [Serializable]
    public class UnSolvedParametersException : CustomizedSqlParametersException
    {
        public override string Message
        {
            get
            {
                return "无法解析的自定义参数。";
            }
        }
    }
    [Serializable]
    public class UnSolvedInheritedLevelException : CustomizedSqlParametersException
    {
        public override string Message
        {
            get
            {
                return "无法解析的回溯层级。";
            }
        }
    }

    public struct Parameter
    {
        public IJsonTreeNode ref_node;
        public IJsonTreeNode ref_parent;
        public String svalue;
        public bool IsString
        {
            get
            {
                return ref_node == null;
            }
        }
    }
    /// <summary>
    /// 自定义参数标定名称，参数标定格式为">paraA>childB///,paraB/"，其中反斜线数目为父级层次数目，字符串写在最前，代表该表下属性的Json标记名称（注意：不是数据库列名），多个标记之间用逗号隔开
    /// </summary>
    public class CustomizedSqlParameters : ICustomizedSqlParameters
    {
        public List<Parameter> Parameters { get; private set; }
        private string describe;
        
        /// <summary>
        /// 实体搜索接口，用于配置搜索模板
        /// </summary>
        interface EntityFinder
        {
            IJsonTreeNode Process(IJsonTreeNode node);
        }

        /// <summary>
        /// 回溯搜索
        /// </summary>
        class TrackBack : EntityFinder
        {
            public IJsonTreeNode Process(IJsonTreeNode node)
            {
                return node.Parent;
            }
        }

        /// <summary>
        /// 深度搜索
        /// </summary>
        class ZoomIn : EntityFinder
        {
            private string entityName;
            public ZoomIn(string name)
            {
                entityName = name;
            }
            public IJsonTreeNode Process(IJsonTreeNode node)
            {
                IJsonTreeNode res;
                if (!node.ChildNodes.TryGetValue(entityName, out res))
                {
                    throw new UnSolvedInheritedLevelException()
                    {
                        ParaName = entityName,
                        Advice = String.Format("解析时，未在节点{0}下找到{1}实体。", node.JsonNodeName, entityName)
                    };
                }
                return res;
            }
        }

        /// <summary>
        /// 解析字符串中包含的回溯指令
        /// </summary>
        /// <param name="argv">目标字符串</param>
        /// <returns>返回两个队列，一个队列用于回溯寻找父节点，另一个队列用于搜索引用节点</returns>
        private Queue<EntityFinder>[] customizedDynamicParameterCheck(String argv)
        {
            Queue<EntityFinder>[] res = new Queue<EntityFinder>[2] 
                { new Queue<EntityFinder>(), new Queue<EntityFinder>() };
            int i = argv.Length - 1;
            int j = 0;

            // 跳过起始空格
            while (argv[j] == ' ') j++;

            // 先搜索回溯项，为了和旧模式兼容
            while (argv[i] == '/')
            {
                res[0].Enqueue(new TrackBack());
                i--;
            }

            // 搜索递进项
            int entity_start = 0;
            for (; j <= i; ++j)
            {
                if (argv[i] == '>')
                {
                    res[1].Enqueue(new ZoomIn(argv.Substring(entity_start + 1, i - entity_start)));
                    entity_start = i;
                }
                else if (!((argv[i] >= 'A' && argv[i] <= 'Z') || 
                    (argv[i] >= 'a' && argv[i] <= 'z') || 
                    (argv[i] >= '0' && argv[i] <= '9') || argv[i] == '_'))
                {
                    throw new UnSolvedParametersException()
                    {
                        ParaName = argv,
                        Advice = String.Format("非法字符{0}，出现在位置{1}", argv[i], i)
                    };
                }
            }
            return res;
        }
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
            // 检查是否参数为空
            if (!String.IsNullOrWhiteSpace(str))
            {
                // 保留预置字符串信息，用于存储
                describe = str;
                // 分割参数
                string[] paras = str.Split(',');
                // 依次处理每个参数
                foreach (string argv in paras)
                {
                    // 检查是否为参数索引
                    if (argv[0] == '>')
                    {
                        // 用数组保存两个关键节点，第一个元素为最近公共父节点，第二个元素为引用目标元素
                        IJsonTreeNode[] refNode = new IJsonTreeNode[2];
                        try
                        {
                            var qOperations = customizedDynamicParameterCheck(argv);
                            
                            for(int i = 0; i < 2; ++i)
                            {
                                while(qOperations[i].Count != 0)
                                {
                                    refNode[i] = qOperations[i].Dequeue().Process(refNode[i]);
                                }
                                if (i == 0)
                                    refNode[i + 1] = refNode[i];
                            }

                            Parameters.Add(new Parameter() { ref_node = refNode[1], ref_parent = refNode[0] });
                        }
                        catch (CustomizedSqlParametersException e)
                        {
                            e.Node = current.JsonNodeName;
                            throw e;
                        }
                        finally
                        {
                            Parameters.Clear();
                            describe = "";
                        }
                    }
                    else
                    {
                        Parameters.Add(new Parameter() { svalue = argv });
                    }
                }
            }
        }
    }
}
