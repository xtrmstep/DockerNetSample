using System.Threading.Tasks;
using Grpc.Net.Client;
using LoaderClient.Configuration;
using Microsoft.Extensions.Options;
using WorkerService;

namespace LoaderClient.Tasks
{
    public class WorkerTask : IWorkerTask
    {
        private readonly LoaderSettings _settings;

        public WorkerTask(IOptions<LoaderSettings> settings)
        {
            _settings = settings.Value;
        }
        
        public async Task Run()
        {
            using var channel = GrpcChannel.ForAddress(_settings.WorkerServiceEndpoint);
            var client = new Worker.WorkerClient(channel);
            var reply = await client.FactorialAsync(new FactorialRequest {Factor = 10});
        }
    }
}