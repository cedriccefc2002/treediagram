using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TreeDiagram;
using Server.lib.Config;
using Server.lib.Repository;
namespace Server.lib.Service
{
    public class ServerService : TreeDiagram.ServerDisp_
    {
        private readonly ILogger<ServerService> logger;
        public ServerService(ILogger<ServerService> logger)
        {
            this.logger = logger;
        }
        private async Task<ServerStatus> statusAsync()
        {
            await Task.Delay(0);
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            var status = await repository.Status();
            return status ? ServerStatus.Normal : ServerStatus.Fault;
        }

        public override ServerStatus status(Ice.Current current)
        {
            // _logger.LogInformation($"status");
            var task = statusAsync();

            return task.Result;
        }

        public override void createTree(Tree tree, Ice.Current current)
        {
            logger.LogInformation("");
        }

        public override Tree[] readTree(Ice.Current current)
        {
            logger.LogInformation("");
            return new Tree[] { new Tree() };
        }

        public override Tree readSingleTree(string uuid, Ice.Current current)
        {
            logger.LogInformation("");
            return new Tree() { uuid = uuid };
        }
        public override void updateTree(Tree tree, Ice.Current current)
        {
            logger.LogInformation("");
        }

        public override void deleteTree(string uuid, Ice.Current current)
        {
            logger.LogInformation("");
        }
    }
}