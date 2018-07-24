using System;
using System.Collections.Generic;

namespace DBToJsonProject.Controller.SettingManager
{
    public interface IJsonTreeNode
    {
        String JsonNodeName { get; }
        Boolean VirtualNode { get; }
        String DbName { get; }
        String DisplayName { get; }
        Boolean MultiRelated { get; }
        Boolean BuildSingleFile { get; }
        Boolean Selectable { get; }
        ICustomizedSqlDescriber Sql { get; set; }
        Dictionary<String, IJsonTreeNode> ChildNodes { get; }
        IJsonTreeNode Parent { get; }
        bool IsDBColumn { get; }
        bool Equals(IJsonTreeNode obj);
        bool HasVirtualNode { get; }
        bool IsDbTable { get; }
    }
}
