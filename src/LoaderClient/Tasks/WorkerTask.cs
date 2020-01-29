using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using JustEat.StatsD;
using LoaderClient.Configuration;
using Microsoft.Extensions.Options;
using WorkerService;

namespace LoaderClient.Tasks
{
    public class WorkerTask : IWorkerTask
    {
        private readonly IStatsDPublisher _stats;
        private readonly LoaderSettings _settings;
        private readonly SemaphoreSlim _semaphoreSlim;

        public WorkerTask(IOptions<LoaderSettings> settings, IStatsDPublisher stats)
        {
            _stats = stats;
            _settings = settings.Value;
            ThreadPool.GetMaxThreads(out var maxParallelization, out _);
            _semaphoreSlim = new SemaphoreSlim(_settings.Parallelization == 0 ? maxParallelization : _settings.Parallelization);
        }
        
        public async Task Run()
        {
            await Task.Yield();
            
            var sw = new Stopwatch();
            sw.Start();
            do
            {
                await Task.Delay(100);
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    ThreadPool.GetAvailableThreads(out var availableThreads, out var _);
                    _stats.Gauge(availableThreads, "GaugeAvailableThreads");
                    
                    _stats.Increment("CountRequests");
                    await _stats.Time("TimeWait", async f => await _semaphoreSlim.WaitAsync());

                    try
                    {
                        var parameter = new Random(DateTime.Now.Millisecond).Next(20);
                        using var channel = GrpcChannel.ForAddress(_settings.WorkerServiceEndpoint);
                        var client = new Worker.WorkerClient(channel);
                        var reply = await client.FactorialAsync(new FactorialRequest {Factor = parameter});
                        _stats.Increment("CountProcessed");
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    } 
                });
            } while (_settings.LoadSecondsInterval == 0 || (sw.ElapsedMilliseconds <= _settings.LoadSecondsInterval * 1000));
            sw.Stop();
        }
    }
}