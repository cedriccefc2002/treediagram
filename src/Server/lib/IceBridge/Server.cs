using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ice;
using TreeDiagram;
using Server.lib;
namespace Server.lib.IceBridge
{
    public class Server : TreeDiagram.ServerDisp_
    {
        private readonly ILogger<Server> logger;
        public Server(ILogger<Server> logger)
        {
            this.logger = logger;
        }

        public override ServerStatus status(Current current)
        {
            logger.LogInformation($"status");
            var service = lib.Provider.serviceProvider.GetRequiredService<lib.Service.ServerService>();
            return service.Status().Result ? ServerStatus.Normal : ServerStatus.Fault; ;
        }

        public override void createTree(Tree tree, Current current)
        {
            logger.LogInformation("");
        }

        public override Tree[] readTree(Current current)
        {
            logger.LogInformation("");
            return new Tree[] { new Tree() };
        }

        public override Tree readSingleTree(string uuid, Current current)
        {
            logger.LogInformation("");
            return new Tree() { uuid = uuid };
        }
        public override void updateTree(Tree tree, Current current)
        {
            logger.LogInformation("");
        }

        public override void deleteTree(string uuid, Current current)
        {
            logger.LogInformation("");
        }
    }
}