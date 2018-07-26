namespace DBToJsonProject.Controller.SettingManager
{
    public class CustomizedSqlDescriber : ICustomizedSqlDescriber
    {
        public bool HasCustomizeSQLString
        {
            get
            {
                return !string.IsNullOrEmpty(CustomizeSQLString);
            }
        }
        private string customizedsql;
        public string CustomizeSQLString
        {
            get
            {
                return customizedsql;
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
            CustomizeSQLString = string.Empty;
            param = new CustomizedSqlParameters();
        }
        public CustomizedSqlDescriber(bool hasSql, string sql, string args, IJsonTreeNode current)
        {
            if (hasSql)
                customizedsql = sql;
            else
                CustomizeSQLString = string.Empty;
            param = new CustomizedSqlParameters(args, current);
        }
    }
}
