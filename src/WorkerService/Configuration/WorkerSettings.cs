namespace WorkerService.Configuration
{
    public class WorkerSettings
    {
        public int WorkProcessDelay { get; set; } = 1000;
        public int Parallelization { get; set; } = 0;
    }
}