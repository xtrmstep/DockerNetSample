using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using JustEat.StatsD;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkerService.Configuration;

namespace WorkerService.Services
{
    public class WorkerService : Worker.WorkerBase
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly IStatsDPublisher _stats;
        private readonly WorkerSettings _settings;

        public WorkerService(ILogger<WorkerService> logger, IOptions<WorkerSettings> settings, IStatsDPublisher stats)
        {
            _logger = logger;
            _stats = stats;
            _settings = settings.Value;
            ThreadPool.GetMaxThreads(out var maxParallelization, out _);
            _semaphoreSlim = new SemaphoreSlim(_settings.Parallelization == 0 ? maxParallelization : _settings.Parallelization);
        }

        public override async Task<FactorialReply> Factorial(FactorialRequest request, ServerCallContext context)
        {
            // Obtain the number of available threads in ThreadPool
            ThreadPool.GetAvailableThreads(out var availableThreads, out _);
            // The number of available threads is the example of Gauge metric
            // Send gauge metric to StatsD (using JustEat.StatsD nuget)
            _stats.Gauge(availableThreads, "GaugeAvailableThreads");
            
            // Increment a counter metric for incoming requests
            _stats.Increment("CountRequests");
            
            // The method _stats.Time() will calculate the time while the _semaphoreSlim.WaitAsync() were waiting
            // and send the metric to StatsD
            await _stats.Time("TimeWait", async f => await _semaphoreSlim.WaitAsync());
            
            // Again measure time length of calculation and send it to StatsD 
            var result = await _stats.Time("TimeCalculation", async t => await CalculateFactorialAsync(request.Factor));
            
            // Increment a counter of processed requests
            _stats.Increment("CountProcessed");
            
            return await Task.FromResult(new FactorialReply
            {
                Result = result
            });
        }

        private async Task<long> CalculateFactorialAsync(int factor)
        {
            long result = 1;
            for (var i = 1; i <= factor; i++)
            {
                await Task.Delay(_settings.WorkProcessDelay);
                result *= i;
            }
            return result;
        }
    }
}