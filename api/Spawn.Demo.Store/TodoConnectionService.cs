using Npgsql;

namespace Spawn.Demo.Store
{
    public class TodoConnectionService
    {
        private readonly string _connString;
        
        public TodoConnectionService(string connString)
        {
            _connString = connString;
        }

        public string ConnString
        {
            get
            {
                var pgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(_connString)
                {
                    TrustServerCertificate = true,
                };
                return pgsqlConnectionStringBuilder.ToString();
            }
        }
    }
}