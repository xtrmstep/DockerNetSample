using System.Threading.Tasks;

namespace LoaderClient.Tasks
{
    public interface IWorkerTask
    {
        Task Run();
    }
}