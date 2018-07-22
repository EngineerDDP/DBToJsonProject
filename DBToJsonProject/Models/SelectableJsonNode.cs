using DBToJsonProject.Controller.SettingManager;

namespace DBToJsonProject.Models
{
    public class SelectableJsonNode : NotifyProperty
    {
        public SelectableJsonNode(string name, IJsonTreeNode node)
        {
            Name = name;
            Node = node;
            IsChecked = false;
        }

        public string Name { get; set; }
        public IJsonTreeNode Node { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                UpdatePropertyChange("IsChecked");
            }
        }
    }
}
