using Npgsql;

namespace Spawn.Demo.Store
{
    public class TodoConnectionService
    {   
        private readonly bool _withPooling;
        private readonly string _connString;

        public TodoConnectionService(string connString, bool withPooling)
        {
            _withPooling = withPooling;
            _connString = connString;
        }

        public string ConnString
        {
            get
            {
                var pgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(_connString)
                {
                    TrustServerCertificate = true,
                    Pooling = _withPooling
                };
                return pgsqlConnectionStringBuilder.ToString();
            }
        }
    }
}