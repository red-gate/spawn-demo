namespace Spawn.Demo.Store
{
    public class AccountConnectionService
    {
        public AccountConnectionService(string connString)
        {
            ConnString = connString;
        }

        public string ConnString { get; }
    }
}