namespace DBToJsonProject.Controller.SettingManager
{
    public class CustomizedSqlDescriber : ICustomizedSqlDescriber
    {
        public bool HasCustomizeSQLString { get; private set; }
        private string customizedsql;
        public string CustomizeSQLString
        {
            get
            {
                return HasCustomizeSQLString ? customizedsql : "";
            }
            set
            {
                customizedsql = value;
            }
        }
        private ICustomizedSqlParameters param;
        public ICustomizedSqlParameters Params
        {
            get
            {
                return param;
            }
            set
            {
                param = value;
            }
        }
        public CustomizedSqlDescriber()
        {
            HasCustomizeSQLString = false;
            param = new CustomizedSqlParameters();
        }
        public CustomizedSqlDescriber(bool hasSql, string sql, string args, IJsonTreeNode current)
        {
            if (hasSql)
                customizedsql = sql;

            HasCustomizeSQLString = hasSql;
            param = new CustomizedSqlParameters(args, current);
        }
    }
}
