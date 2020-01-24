using JustEat.StatsD;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerService.Configuration;

namespace WorkerService
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var statsDSettings = _configuration.GetSection(nameof(StatsDSettings));

            services.AddGrpc();
            services.Configure<WorkerSettings>(_configuration.GetSection(nameof(WorkerSettings)));
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Services.WorkerService>();

                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909"); });
            });
        }
    }
}