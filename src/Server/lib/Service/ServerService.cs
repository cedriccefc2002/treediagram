using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TreeDiagram;
namespace Server.lib.Service
{
    public class ServerService : TreeDiagram.ServerDisp_
    {
        private readonly ILogger<ServerService> _logger;
        public ServerService(ILogger<ServerService> logger)
        {
            _logger = logger;
        }
        private async Task<ServerStatus> statusAsync()
        {
            await Task.Delay(0);
            return ServerStatus.Normal;
        }

        public override ServerStatus status(Ice.Current current)
        {
            var task = statusAsync();
            return task.Result;
        }
    }
}