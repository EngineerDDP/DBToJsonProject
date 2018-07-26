using DBToJsonProject.Controller.SettingManager;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DBToJsonProject.Models
{
    public class SelectableJsonList : NotifyProperty
    {
        public SelectableJsonList(string name, IJsonTreeNode node)
        {
            Name = name;
            Node = node;
            Nodes = new ObservableCollection<SelectableJsonNode>();
            Nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        private void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.UpdatePropertyChange("Nodes");
        }
        public IJsonTreeNode Node { get; set; }
        public string Name { get; set; }
        public ObservableCollection<SelectableJsonNode> Nodes { get; set; }
    }
}
