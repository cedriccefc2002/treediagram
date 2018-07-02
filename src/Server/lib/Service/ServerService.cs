using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.lib.Config;
using Server.lib.Repository;
using Server.lib.Service.Model;
namespace Server.lib.Service
{
    public class ServerService : IServerService
    {
        private readonly ILogger<ServerService> logger;
        private readonly EventService eventService;
        private readonly IRepository repository;
        private readonly IList<string> Clients = new List<string>();
        private readonly IDictionary<string, string> TreeEditLock = new Dictionary<string, string>();
        private readonly IDictionary<string, string> ClientEditLock = new Dictionary<string, string>();
        public ServerService(ILogger<ServerService> logger)
        {
            this.logger = logger;
            eventService = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
            repository = lib.Provider.serviceProvider.GetRequiredService<IRepository>();
            eventService.evtClientConnect += async uuid =>
            {
                await Task.Yield();
                Clients.Add(uuid);
            };
            eventService.evtClientDisConnect += async clientUUID =>
            {
                await Task.Yield();
                var treeUUID = "";
                if (ClientEditLock.TryGetValue(clientUUID, out treeUUID))
                {
                    ClientEditLock.Remove(clientUUID);
                    TreeEditLock.Remove(treeUUID);
                }
                Clients.Remove(clientUUID);
            };
            eventService.evtTreeEditLock += async (string treeUUID, string clientUUID) =>
            {
                await Task.Yield();
                TreeEditLock.Add(treeUUID, clientUUID);
                ClientEditLock.Add(clientUUID, treeUUID);
            };
            eventService.evtTreeEditRelease += async (string treeUUID) =>
            {
                await Task.Yield();
                var clientUUID = "";
                if (TreeEditLock.TryGetValue(treeUUID, out clientUUID))
                {
                    ClientEditLock.Remove(clientUUID);
                }
                TreeEditLock.Remove(treeUUID);
            };
        }

        public async Task<bool> Status()
        {
            return await repository.Status();
        }
        public async Task<bool> createTree(Model.TreeModel tree)
        {
            var result = await repository.CreateTree(tree.TreeDomain());
            var t = eventService.DoTreeListUpdate();
            return result;
        }
        private async Task<bool> IsFree(string treeUUID, string clientUUID)
        {
            await Task.Yield();
            var lockClientUUID = "";
            if (TreeEditLock.TryGetValue(treeUUID, out lockClientUUID))
            {
                return lockClientUUID == clientUUID;
            }
            else
            {
                return true;
            }
        }
        private async Task<bool> DoTreeUpdate(string clientUUID, string treeUUID, Func<Task<bool>> process)
        {
            if (await IsFree(clientUUID, treeUUID))
            {
                await eventService.DoTreeEditLock(clientUUID, treeUUID);
                var result = await process();
                await eventService.DoTreeEditRelease(treeUUID);
                return result;
            }
            else
            {
                return false;
            }
        }
        public async Task<List<Model.TreeModel>> ListAllTrees()
        {
            return (await repository.ListAllTrees()).Select(a => Model.TreeModel.FromDomain(a)).ToList();
        }

        public async Task<bool> DeleteTree(string uuid)
        {
            var result = await repository.DeleteTree(uuid);
            var t = eventService.DoTreeListUpdate();
            return result;
        }

        public async Task<TreeModel> GetTreeByUUID(string uuid)
        {
            return TreeModel.FromDomain(await repository.GetTreeByUUID(uuid));
        }
        public async Task<NodeModel> GetNodeByUUID(string uuid)
        {
            return NodeModel.FromDomain(await repository.GetNodeByUUID(uuid));
        }

        public async Task<uint> GetChildrenCount(string uuid)
        {
            return await repository.GetChildrenCount(uuid);
        }
        public async Task<bool> CreateNode(string clientUUID, string rootUUID, string parentUUID, string data)
        {
            return await DoTreeUpdate(clientUUID, rootUUID, async () =>
            {
                bool result = false;
                if (await IsFree(clientUUID, rootUUID))
                {
                    var root = await GetTreeByUUID(rootUUID);
                    if (root.type == TreeType.Binary)
                    {
                        var childern = await repository.GetChildrenNode(parentUUID);
                        if (childern.Count >= 2)
                        {
                            result = false;
                        }
                        else if (childern.Count == 1)
                        {
                            var IsBinaryleft = NodeModel.IsBinaryleft(childern.First().isBinaryleft);
                            await repository.CreateNode(rootUUID, parentUUID, data, NodeModel.IsBinaryleft(!IsBinaryleft));
                            result = true;
                        }
                        else
                        {
                            await repository.CreateNode(rootUUID, parentUUID, data, NodeModel.IsBinaryleft(true));
                            result = true;
                        }
                    }
                    else
                    {
                        await repository.CreateNode(rootUUID, parentUUID, data, NodeModel.IsBinaryleft(false));
                        result = true;
                    }
                    if (result)
                    {
                        var t = eventService.DoTreeEditFinish(rootUUID);
                    }
                }
                return result;
            });
        }

        public async Task<List<Model.NodeModel>> GetChildrenNode(string uuid)
        {
            return (await repository.GetChildrenNode(uuid)).Select(a => Model.NodeModel.FromDomain(a)).ToList();
        }

        public async Task<bool> UpdateNodeData(string clientUUID, string rootUUID, string data)
        {
            return await DoTreeUpdate(clientUUID, rootUUID, async () =>
               {
                   var result = (await repository.UpdateNodeData(rootUUID, data));
                   var t = eventService.DoNodeUpdate(rootUUID, data, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());
                   return result;
               });
        }

        public async Task<bool> DeleteNodeTree(string clientUUID, string rootUUID)
        {
            return await DoTreeUpdate(clientUUID, rootUUID, async () =>
               {
                   var node = await GetNodeByUUID(rootUUID);
                   var result = await repository.DeleteNodeTree(rootUUID);
                   var t = eventService.DoTreeEditFinish(node.root);
                   return result;
               });
        }

        public async Task<bool> MoveNode(string clientUUID, string rootUUID, string newParent)
        {
            return await DoTreeUpdate(clientUUID, rootUUID, async () =>
               {
                   var node = await GetNodeByUUID(rootUUID);
                   var root = await GetTreeByUUID(node.root);
                   bool result = false;
                   if (root.type == TreeType.Binary)
                   {
                       var count = await GetChildrenCount(newParent);
                       if (count >= 2)
                       {
                           result = false;
                       }
                       else if (count == 1)
                       {
                           await repository.MoveNode(rootUUID, newParent, NodeModel.IsBinaryleft(false));
                           result = true;
                       }
                       else
                       {
                           await repository.MoveNode(rootUUID, newParent, NodeModel.IsBinaryleft(true));
                           result = true;
                       }
                   }
                   else
                   {
                       await repository.MoveNode(rootUUID, newParent, NodeModel.IsBinaryleft(false));
                       result = true;
                   }
                   if (result)
                   {
                       var t = eventService.DoTreeEditFinish(root.uuid);
                   }
                   return result;
               });
        }

        public async Task<bool> DeleteNode(string clientUUID, string rootUUID)
        {
            return await DoTreeUpdate(clientUUID, rootUUID, async () =>
               {
                   var node = await GetNodeByUUID(rootUUID);
                   var root = await GetTreeByUUID(node.root);
                   bool result = false;
                   if (root.type == TreeType.Binary)
                   {
                       var count = (await GetChildrenCount(node.uuid)) + (await GetChildrenCount(node.parent));
                       if (count > 2)
                       {
                           result = false;
                       }
                       else
                       {
                           var childern = await repository.GetChildrenNode(node.uuid);
                           foreach (var child in childern)
                           {
                               await repository.MoveNode(rootUUID, node.parent, NodeModel.IsBinaryleft(node.isBinaryleft));
                           }
                           await repository.DeleteNode(rootUUID);
                           result = true;
                       }
                   }
                   else
                   {
                       await repository.DeleteNode(rootUUID);
                       result = true;
                   }
                   if (result)
                   {
                       var t = eventService.DoTreeEditFinish(root.uuid);
                   }
                   return result;
               });
        }
        public async Task<TreeViewModel> GetNodeView(string uuid)
        {
            return TreeViewModel.FromDomain(await repository.GetNodeView(uuid));
        }
    }
}