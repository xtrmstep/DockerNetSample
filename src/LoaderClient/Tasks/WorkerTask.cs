using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
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
        private SemaphoreSlim _semaphoreSlim;
        public ConcurrentQueue<Task<FactorialReply>> _tasks { get; } = new ConcurrentQueue<Task<FactorialReply>>();

        public WorkerTask(IOptions<LoaderSettings> settings)
        {
            _settings = settings.Value;
            ThreadPool.GetMaxThreads(out int maxParallelization, out _);
            _semaphoreSlim = new SemaphoreSlim(_settings.Parallelization == 0 ? maxParallelization : _settings.Parallelization);
        }
        
        public async Task Run()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            do
            {
                var task = ThreadPool.QueueUserWorkItem(async _ =>
                {
                    await _semaphoreSlim.WaitAsync();

                    using var channel = GrpcChannel.ForAddress(_settings.WorkerServiceEndpoint);
                    var client = new Worker.WorkerClient(channel);
                    var reply = client.FactorialAsync(new FactorialRequest { Factor = 10 });
                    _tasks.Enqueue(reply.ResponseAsync);
                });
            } while (sw.ElapsedMilliseconds <= _settings.LoadSecondsInterval * 1000);
            await Task.WhenAll(_tasks);
        }
    }
}