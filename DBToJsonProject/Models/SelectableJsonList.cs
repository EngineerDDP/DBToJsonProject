using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DBToJsonProject.Models
{
    public class SelectableJsonList : NotifyProperty
    {
        public SelectableJsonList(string name)
        {
            Name = name;
            Nodes = new ObservableCollection<SelectableJsonNode>();
            Nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        private void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.UpdatePropertyChange("Nodes");
        }

        public string Name { get; set; }
        public ObservableCollection<SelectableJsonNode> Nodes { get; set; }
    }
}
