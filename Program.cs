using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;


namespace SmithSmsStatusFetcher
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            Startup startup = new Startup();

            var builder = new HostBuilder()
                .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production") //load appropriate environment settings; doesnt use ASPNETCORE_ENVIRONMENT by default
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                    .AddUserSecrets<Program>();

                    if (args != null) config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services = startup.ConfigureServices(context, services);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConfiguration(context.Configuration);
                    logging.AddConsole();
                });

            await startup.Run(builder);

        }
    }
}
