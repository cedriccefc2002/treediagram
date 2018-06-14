using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ice;
using TreeDiagram;
using Service = Server.lib.Service;
using Model = Server.lib.Service.Model;
using Domain = Server.lib.Repository.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            return service.Status().Result ? ServerStatus.Normal : ServerStatus.Fault; ;
        }

        public override void createTree(Tree tree, Current current)
        {
            logger.LogInformation("createTree");
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            service.createTree(new Model.TreeModel()
            {
                uuid = tree.uuid,
                type = tree.type == TreeType.Binary ? Domain.TreeType.Binary : Domain.TreeType.Normal
            }).Wait();
        }

        public override Tree[] readTree(Current current)
        {
            logger.LogInformation("readTree");
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            return service.readTree().Result.Select(a => new Tree()
            {
                uuid = a.uuid,
                type = a.type == Domain.TreeType.Binary ? TreeType.Binary : TreeType.Normal
            }).ToArray();
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
            logger.LogInformation("deleteTree");
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            service.DeleteTree(uuid).Wait();
        }
    }
}