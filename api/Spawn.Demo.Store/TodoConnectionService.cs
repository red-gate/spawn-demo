namespace Spawn.Demo.Store
{
    public class TodoConnectionService
    {
        public TodoConnectionService(string connString)
        {
            ConnString = connString;
        }

        public string ConnString { get; }
    }
}