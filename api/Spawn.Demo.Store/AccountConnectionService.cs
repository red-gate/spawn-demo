using System.Data.SqlClient;

namespace Spawn.Demo.Store
{
    public class AccountConnectionService
    {
        private readonly bool _withPooling;
        private readonly string _connectionString;

        public AccountConnectionService(string connString, bool withPooling)
        {
            _withPooling = withPooling;
            _connectionString = connString;
        }

        public string ConnString 
        {
            get
            {
                var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString)
                {
                    TrustServerCertificate = true,
                    Pooling = _withPooling
                };
                return sqlConnectionStringBuilder.ToString();
            }
        }
    }
}