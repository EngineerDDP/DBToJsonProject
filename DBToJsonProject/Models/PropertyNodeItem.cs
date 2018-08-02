using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DBToJsonProject.Models
{
    /// <summary>
    /// 用于和前端交互的类。表示了Json结构与数据库结构的映射关系
    /// </summary>
    internal class PropertyNodeItem : NotifyProperty
    {
        /// <summary>
        /// 节点名称
        /// </summary>
        private String name;
        /// <summary>
        /// 节点显示名称
        /// </summary>
        public String DisplayName { get; set; }
        /// <summary>
        /// 节点是否被展开
        /// </summary>
        private Boolean isExpanded;
        public Boolean IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                isExpanded = value;
                UpdatePropertyChange("IsExpanded");
            }
        }
        public String JsonName {
            get
            {
                return name;
            }
            set
            {
                name = value;
                UpdatePropertyChange("JsonName");
            }
        }
        public Boolean VirtualNode { get; set; }
        public String EntityName { get; set; }
        public Boolean MultiReleationShip { get; set; }
        public Boolean BuildJson { get; set; }
        public Boolean HasChildren { get; set; }
        public Boolean HasCustomizedSql { get; set; }
        public Boolean Selectable { get; set; }
        public String CustomizedSql { get; set; }
        public String CustomizedSqlParameters { get; set; }
        private ObservableCollection<PropertyNodeItem> childNodes;
        public ObservableCollection<PropertyNodeItem> Childs
        {
            get
            {
                return childNodes;
            }
            set
            {
                childNodes = value;
                UpdatePropertyChange("Childs");
            }
        }
        public PropertyNodeItem Parent { get; set; }
        /// <summary>
        /// 按照默认模板新建
        /// </summary>
        private static uint C = 0;
        public static PropertyNodeItem Default
        {
            get
            {
                C += 1;
                return new PropertyNodeItem()
                {
                    DisplayName = String.Format("DisplayName{0}", C),
                    JsonName = String.Format("JsonName{0}", C),
                    EntityName = String.Format("EntityName{0}", C),
                    IsExpanded = false,
                    MultiReleationShip = false,
                    BuildJson = false,
                    HasChildren = false,
                };
            }
        }
        private PropertyNodeItem()
        {
            Parent = null;
            childNodes = new ObservableCollection<PropertyNodeItem>();
            childNodes.CollectionChanged += ChildNodes_CollectionChanged;
        }

        private void ChildNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdatePropertyChange("Childs");
        }
        public override bool Equals(object obj)
        {
            if(this.GetType() == obj.GetType())
            {
                var t = obj as PropertyNodeItem;
                if (this.EntityName == t.EntityName && this.JsonName == t.JsonName)
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 237926843;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
            hashCode = hashCode * -1521134295 + IsExpanded.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(JsonName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(EntityName);
            hashCode = hashCode * -1521134295 + MultiReleationShip.GetHashCode();
            hashCode = hashCode * -1521134295 + BuildJson.GetHashCode();
            return hashCode;
        }
    }
}
