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
            // _logger.LogInformation($"status");
            var task = statusAsync();

            return task.Result;
        }

        public override void createTree(Tree tree, Ice.Current current)
        {
            _logger.LogInformation("");
        }

        public override Tree[] readTree(Ice.Current current)
        {
            _logger.LogInformation("");
            return new Tree[] { new Tree() };
        }

        public override Tree readSingleTree(string uuid, Ice.Current current)
        {
            _logger.LogInformation("");
            return new Tree() { uuid = uuid };
        }
        public override void updateTree(Tree tree, Ice.Current current)
        {
            _logger.LogInformation("");
        }

        public override void deleteTree(string uuid, Ice.Current current)
        {
            _logger.LogInformation("");
        }
    }
}