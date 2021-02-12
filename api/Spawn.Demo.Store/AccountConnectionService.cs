using System.Data.SqlClient;

namespace Spawn.Demo.Store
{
    public class AccountConnectionService
    {
        private readonly string _connectionString;

        public AccountConnectionService(string connString)
        {
            _connectionString = connString;
        }

        public string ConnString {
            get
            {
                var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString)
                {
                    TrustServerCertificate = true
                };
                return sqlConnectionStringBuilder.ToString();
            }
        }
    }
}