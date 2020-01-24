using System;
using System.IO;
using System.Threading.Tasks;
using JustEat.StatsD;
using LoaderClient.Configuration;
using LoaderClient.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoaderClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = CreateConfiguration(environment);
            var serviceProvider = ConfigureServices(configuration);

            var runner = serviceProvider.GetService<IWorkerTask>();
            await runner.Run();
        }

        private static IConfigurationRoot CreateConfiguration(string environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true);
            var configuration = builder.Build();
            return configuration;
        }

        private static ServiceProvider ConfigureServices(IConfigurationRoot configuration)
        {
            var statsDSettings = configuration.GetSection(nameof(StatsDSettings));

            var services = new ServiceCollection().AddOptions();
            services.AddStatsD(
                    provider =>
                    {
                        var logger = provider.GetService<ILogger<Program>>();
                        return new StatsDConfiguration
                        {
                            Host = statsDSettings["HostName"],
                            Port = int.Parse(statsDSettings["Port"]),
                            Prefix = statsDSettings["Prefix"],
                            OnError = ex =>
                            {
                                logger.LogError(ex, ex.Message);
                                //return true; // ignore the exception
                                return false;
                            }
                        };
                    });

            var configurationSection = configuration.GetSection(nameof(LoaderSettings));
            services.Configure<LoaderSettings>(configurationSection);
            services.AddTransient<IWorkerTask, WorkerTask>();

            return services.BuildServiceProvider();
        }
    }
}