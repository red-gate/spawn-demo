using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Spawn.Demo.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var bindingConfig = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureAppConfiguration(cfgBuilder =>
                       {
                           cfgBuilder.AddJsonFile("appsettings.Development.Database.json", optional: true);
                       })
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                           webBuilder.UseStartup<Startup>();
                       });
        }
    }
}