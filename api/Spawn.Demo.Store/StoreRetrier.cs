using System;
using System.Threading.Tasks;

namespace Spawn.Demo.Store
{
    public abstract class StoreRetrier
    {
        private readonly int connection_retries = 10;
        private readonly int wait_between_connections = 2000;

        protected async Task<T> RunWithRetryAsync<T>(Func<T> func)
        {
            for (var i = 0; i < connection_retries; i++)
            {
                if (CanConnect())
                {
                    return func();
                }

                Console.WriteLine(
                    $"Unable to connect to database, waiting {wait_between_connections}ms before retrying...");
                await Task.Delay(wait_between_connections);
            }

            throw new TimeoutException($"Unable to connect to database after {connection_retries} attempts");
        }

        protected async Task RunWithRetryAsync(Action action)
        {
            Func<int> func = () =>
            {
                action();
                return 0;
            };
            await RunWithRetryAsync(() => func());
        }

        protected abstract bool CanConnect();
    }
}