namespace LoaderClient.Configuration
{
    public class LoaderSettings
    {
        public string WorkerServiceEndpoint { get; set; }
        public int LoadSecondsInterval { get; set; } = 0; // no limit
        public int Parallelization { get; set; } = 0; // no limit
    }
}