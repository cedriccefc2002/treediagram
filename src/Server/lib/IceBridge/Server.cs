using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ice;
using TreeDiagram;
using Service = Server.lib.Service;
using Model = Server.lib.Service.Model;
using Domain = Server.lib.Repository.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Server.lib.IceBridge
{
    public class Server : TreeDiagram.ServerDisp_
    {
        private readonly ILogger<Server> logger;
        private readonly IList<ServerEventPrxHelper> clients = new List<ServerEventPrxHelper>();
        public Server(ILogger<Server> logger)
        {
            this.logger = logger;
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
            service.evtTreeListUpdate += new Service.TreeListUpdateDelegate(TreeListUpdateHandler);
            service.evtTreeUpdate += new Service.TreeUpdateDelegate(TreeUpdateHandler);
            service.evtNodeUpdate += new Service.NodeUpdateDelegate(NodeUpdateHandler);
        }

        public override ServerStatus status(Current current)
        {
            logger.LogInformation($"");
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            return service.Status().Result ? ServerStatus.Normal : ServerStatus.Fault; ;
        }

        public override void createTree(Tree tree, Current current)
        {
            logger.LogInformation("");
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            service.createTree(new Model.TreeModel()
            {
                uuid = tree.uuid,
                type = tree.type == TreeType.Binary ? Model.TreeType.Binary : Model.TreeType.Normal
            }).Wait();
        }

        public override Tree[] readTree(Current current)
        {
            logger.LogInformation("");
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            return service.ListAllTrees().Result.Select(a => new Tree()
            {
                uuid = a.uuid,
                type = a.type == Model.TreeType.Binary ? TreeType.Binary : TreeType.Normal
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
            logger.LogInformation("");
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            service.DeleteTree(uuid).Wait();
        }

        private void ClientEventHandler(Func<ServerEventPrxHelper, Task> act)
        {
            var removeList = new List<ServerEventPrxHelper>();
            foreach (var client in clients)
            {
                // logger.LogInformation($"{client.ice_getConnectionId()}");
                try
                {
                    act(client);
                }
                catch (System.Exception ex)
                {
                    logger.LogError(ex.Message);
                    removeList.Add(client);
                }
            }
            removeList.ForEach(c => clients.Remove(c));
        }
        private void TreeListUpdateHandler()
        {
            ClientEventHandler(async c => await c.TreeListUpdateAsync());
        }
        private void TreeUpdateHandler(string uuid)
        {
            ClientEventHandler(async c => await c.TreeUpdateAsync(uuid));
        }
        private void NodeUpdateHandler(string uuid, string data)
        {
            ClientEventHandler(async c => await c.NodeUpdateAsync(uuid, data));
        }
        public override void initEvent(ServerEventPrx serverEvent, Current current)
        {
            clients.Add(((ServerEventPrxHelper)serverEvent));
            var service = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
            var task = service.DoTreeListUpdate();
            // Task.Run(async () =>
            // {
            //     while (true)
            //     {
            //         try
            //         {
            //             ((ServerEventPrxHelper)serverEvent).TreeListUpdate();
            //             await Task.Delay(6000);
            //         }
            //         catch (Exception ex)
            //         {
            //             logger.LogError(ex.Message);
            //             throw ex;
            //         }
            //     }
            // });
            logger.LogInformation("");
        }
    }
}