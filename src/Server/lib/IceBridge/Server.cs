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

        private readonly Service.ServerService service;
        public Server(ILogger<Server> logger)
        {
            this.logger = logger;
            service = lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>();
            var eventService = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
            eventService.evtTreeListUpdate += new Service.TreeListUpdateDelegate(TreeListUpdateHandler);
            eventService.evtTreeUpdate += new Service.TreeUpdateDelegate(TreeUpdateHandler);
            eventService.evtNodeUpdate += new Service.NodeUpdateDelegate(NodeUpdateHandler);
        }

        public override void createTree(Tree tree, Current current)
        {
            logger.LogInformation($"{tree.uuid}");
            service.createTree(new Model.TreeModel()
            {
                uuid = tree.uuid,
                type = tree.type == TreeType.Binary ? Model.TreeType.Binary : Model.TreeType.Normal
            }).Wait();
        }

        public override Tree[] listAllTrees(Current current)
        {
            logger.LogInformation("");
            return service.ListAllTrees().Result.Select(a => new Tree()
            {
                uuid = a.uuid,
                type = a.type == Model.TreeType.Binary ? TreeType.Binary : TreeType.Normal
            }).ToArray();
        }

        public override Tree getTreeByUUID(string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            var result = service.GetTreeByUUID(uuid).Result;
            return new Tree()
            {
                uuid = result.uuid,
                type = result.type == Model.TreeType.Binary ? TreeType.Binary : TreeType.Normal
            };
        }

        public override void deleteTree(string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            service.DeleteTree(uuid).Wait();
        }

        public override long getChildrenCount(string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            return service.GetChildrenCount(uuid).Result;
        }

        public override void createNode(string rootUUID, string parentUUID, string data, Current current)
        {
            logger.LogInformation($"{rootUUID}|{parentUUID}|{data}");
            service.CreateNode(rootUUID, parentUUID, data).Wait();
        }

        public override Node[] getChildrenNode(string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            return service.GetChildrenNode(uuid).Result.Select(a => new Node()
            {
                uuid = a.uuid,
                root = a.root,
                parent = a.parent,
                data = a.data,
            }).ToArray();
        }

        public override void updateNodeData(string uuid, string data, Current current)
        {
            logger.LogInformation($"{uuid}|{data}");
            service.UpdateNodeData(uuid, data).Wait();
        }

        public override void deleteNodeTree(string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            service.DeleteNodeTree(uuid).Wait();
        }

        public override void moveNode(string uuid, string newParent, Current current)
        {
            logger.LogInformation($"{uuid}");
            service.MoveNode(uuid, newParent).Wait();
        }

        public override void deleteNode(string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            service.DeleteNode(uuid).Wait();
        }

        public override TreeView getNodeView(string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            var result = service.GetNodeView(uuid).Result;
            return new TreeView()
            {
                uuid = result.uuid,
                nodes = result.nodes.Select(a => new Node()
                {
                    uuid = a.uuid,
                    root = a.root,
                    parent = a.parent,
                    data = a.data,
                }).ToArray(),
                rels = result.rels.Select(a => new NodeRelationship()
                {
                    parentUUID = a.parentUUID,
                    childUUID = a.childUUID,
                }).ToArray(),
            };
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
            // var task = service.DoTreeListUpdate();
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
            logger.LogInformation($"{serverEvent.ice_getAdapterId()}");
        }
    }
}