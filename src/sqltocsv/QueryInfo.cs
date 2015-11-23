namespace sqltocsv
{
    public class QueryInfo
    {
        public QueryInfo(string connectionString, string sqlText)
        {
            ConnectionString = connectionString;
            SqlText = sqlText;
        }

        public string ConnectionString { get; private set; }

        public string SqlText { get; private set; }
    }
}