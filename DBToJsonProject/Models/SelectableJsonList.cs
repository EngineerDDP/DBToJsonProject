using DBToJsonProject.Controller.SettingManager;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DBToJsonProject.Models
{
    /// <summary>
    /// 用于和前端交互的模型，表示同一系列的所有选项
    /// </summary>
    public class SelectableJsonList : NotifyProperty
    {
        public SelectableJsonList(string name, IJsonTreeNode node)
        {
            Name = name;
            Node = node;
            nodes = new ObservableCollection<SelectableJsonNode>();
        }
        public IJsonTreeNode Node { get; set; }
        public string Name { get; set; }
        private ObservableCollection<SelectableJsonNode> nodes;
        public ObservableCollection<SelectableJsonNode> Nodes
        {
            get
            {
                return nodes;
            }
        }
        public void AddNode(SelectableJsonNode item)
        {
            item.PropertyChanged += N_PropertyChanged;
            nodes.Add(item);
        }
        private void N_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.UpdatePropertyChange(e.PropertyName);
        }

        public bool IsChecked
        {
            get
            {
                foreach(SelectableJsonNode n in Nodes)
                {
                    if (!n.IsChecked)
                        return false;
                }
                return true;
            }
            set
            {
                foreach(SelectableJsonNode n in Nodes)
                {
                    n.IsChecked = value;
                }
            }
        }
    }
}
