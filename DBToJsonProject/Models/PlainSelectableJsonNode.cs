using System;
using System.Collections.Generic;

namespace DBToJsonProject.Models
{
    public class PlainSelectableJsonNode
    {
        private static int index = 0x01c98f2;
        public string Name { get; set; }
        public List<PlainSelectableJsonNode> Childs
        {
            get;set;
        }
        public string Tag {
            get
            {
                index += 1;
                return string.Format("a{0:X8}", index);
            }
        }
        public PlainSelectableJsonNode(string name)
        {
            Name = name;
            Childs = new List<PlainSelectableJsonNode>();
        }
        public PlainSelectableJsonNode(SelectableJsonNode node) : this(node.Name) { }
        public PlainSelectableJsonNode(SelectableJsonList select) : this(select.Name)
        {
            foreach (SelectableJsonNode node in select.Nodes)
                Childs.Add(new PlainSelectableJsonNode(node));
        }
        public static implicit operator PlainSelectableJsonNode(SelectableJsonList list)
        {
            return new PlainSelectableJsonNode(list);
        }
        public static implicit operator PlainSelectableJsonNode(SelectableJsonNode node)
        {
            return new PlainSelectableJsonNode(node);
        }
    }
}
