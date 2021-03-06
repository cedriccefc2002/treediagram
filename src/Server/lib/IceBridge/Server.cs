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
        private Service.EventService eventService;
        private readonly ILogger<Server> logger;
        private readonly IList<ServerEventPrxHelper> clients = new List<ServerEventPrxHelper>();
        private readonly Service.IServerService service;
        public Server(ILogger<Server> logger)
        {
            this.logger = logger;
            service = lib.Provider.serviceProvider.GetRequiredService<Service.IServerService>();
            eventService = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
            eventService.evtTreeListUpdate += new Service.TreeListUpdateDelegate(TreeListUpdateHandler);
            eventService.evtTreeEditLock += new Service.TreeEditLockDelegate(TreeEditLockHandler);
            eventService.evtTreeEditRelease += new Service.TreeEditReleaseDelegate(TreeEditReleaseHandler);
            eventService.evtTreeEditFinish += new Service.TreeEditFinishDelegate(TreeEditFinishHandler);
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

        public override void createNode(string clientUUID, string rootUUID, string parentUUID, string data, Current current)
        {
            logger.LogInformation($"{rootUUID}|{parentUUID}|{data}");
            service.CreateNode(clientUUID, rootUUID, parentUUID, data).Wait();
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
                isBinaryleft = a.isBinaryleft,
            }).ToArray();
        }

        public override void updateNodeData(string clientUUID, string uuid, string data, Current current)
        {
            logger.LogInformation($"{uuid}|{data}");
            service.UpdateNodeData(clientUUID, uuid, data).Wait();
        }

        public override void deleteNodeTree(string clientUUID, string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            service.DeleteNodeTree(clientUUID, uuid).Wait();
        }

        public override void moveNode(string clientUUID, string uuid, string newParent, Current current)
        {
            logger.LogInformation($"{uuid}");
            service.MoveNode(clientUUID, uuid, newParent).Wait();
        }

        public override void deleteNode(string clientUUID, string uuid, Current current)
        {
            logger.LogInformation($"{uuid}");
            service.DeleteNode(clientUUID, uuid).Wait();
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
                    isBinaryleft = a.isBinaryleft,
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
        private async Task TreeListUpdateHandler()
        {
            await Task.Yield();
            ClientEventHandler(async c => await c.TreeListUpdateAsync());
        }
        private async Task TreeEditReleaseHandler(string uuid)
        {
            await Task.Yield();
            ClientEventHandler(async c => await c.TreeEditReleaseAsync(uuid));
        }
        private async Task TreeEditLockHandler(string treeUUID, string clientUUID)
        {
            await Task.Yield();
            ClientEventHandler(async c => await c.TreeEditLockAsync(treeUUID, clientUUID));
        }
        private async Task TreeEditFinishHandler(string uuid)
        {
            await Task.Yield();
            ClientEventHandler(async c => await c.TreeEditFinishAsync(uuid));
        }
        private async Task NodeUpdateHandler(string uuid, string data, long timestamp)
        {
            await Task.Yield();
            ClientEventHandler(async c => await c.NodeUpdateAsync(uuid, data, timestamp));
        }
        public override void initEvent(string clientUUID, ServerEventPrx serverEvent, Current current)
        {
            clients.Add(((ServerEventPrxHelper)serverEvent));
            eventService.DoClientConnect(clientUUID).Wait();
            current.con.setCloseCallback(async (con) =>
            {
                await Task.Yield();
                logger.LogInformation($"{serverEvent.GetHashCode()}");
                clients.Remove((ServerEventPrxHelper)serverEvent);
                await eventService.DoClientDisConnect(clientUUID);
            });
            // var service = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
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
            logger.LogInformation($"{serverEvent.GetHashCode()}");
        }
    }
}