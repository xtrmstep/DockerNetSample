using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace WorkerService
{
    public class WorkerService : Worker.WorkerBase
    {
        private readonly ILogger<WorkerService> _logger;
        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }

        public override Task<LoadReply> Load(LoadRequest request, ServerCallContext context)
        {
            return Task.FromResult(new LoadReply
            {
                NumberOfMessages = 111
            });
        }
    }
}
