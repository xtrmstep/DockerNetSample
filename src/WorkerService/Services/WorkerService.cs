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
        private readonly IStatsDPublisher _stats;
        private readonly WorkerSettings _workerSettings;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10000);

        public WorkerService(ILogger<WorkerService> logger, IOptions<WorkerSettings> workerSettings, IStatsDPublisher stats)
        {
            _logger = logger;
            _stats = stats;
            _workerSettings = workerSettings.Value;
        }

        public override async Task<FactorialReply> Factorial(FactorialRequest request, ServerCallContext context)
        {
            _stats.Increment("FactorialRequest");
            _stats.Gauge(_semaphore.CurrentCount, "AvailableWorkers");
            var result = await _stats.Time("FactorialTime", async t => await CalculateFactorialAsync(request.Factor));
            return await Task.FromResult(new FactorialReply
            {
                Result = result
            });
        }

        private async Task<long> CalculateFactorialAsync(int factor)
        {
            await _semaphore.WaitAsync();

            long result = 1;
            for (var i = 1; i <= factor; i++)
            {
                await Task.Delay(_workerSettings.WorkProcessDelay);
                result *= i;
            }

            return result;
        }
    }
}