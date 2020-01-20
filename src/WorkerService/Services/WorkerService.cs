using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkerService.Configuration;

namespace WorkerService.Services
{
    public class WorkerService : Worker.WorkerBase
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly WorkerSettings _workerSettings;

        public WorkerService(ILogger<WorkerService> logger, IOptions<WorkerSettings> workerSettings)
        {
            _logger = logger;
            _workerSettings = workerSettings.Value;
        }

        public override async Task<FactorialReply> Factorial(FactorialRequest request, ServerCallContext context)
        {
            long result = 1;
            for (var i = 1; i <= request.Factor; i++)
            {
                await Task.Delay(_workerSettings.WorkProcessDelay);
                result *= i;
            }
            return await Task.FromResult(new FactorialReply
            {
                Result = result
            });
        }
    }
}
