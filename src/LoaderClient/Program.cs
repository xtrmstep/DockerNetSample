using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using WorkerService;

namespace LoaderClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Worker.WorkerClient(channel);
            var reply = await client.LoadAsync(new LoadRequest {Seconds = 10});

            Console.WriteLine("Loaded: " + reply.NumberOfMessages);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}