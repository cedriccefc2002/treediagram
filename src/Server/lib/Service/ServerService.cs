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
    public class ServerService
    {
        private readonly ILogger<ServerService> logger;
        private readonly EventService eventService;
        private readonly IRepository repository;
        public ServerService(ILogger<ServerService> logger)
        {
            this.logger = logger;
            eventService = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
            repository = lib.Provider.serviceProvider.GetRequiredService<IRepository>();
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
        public async Task<bool> CreateNode(string rootUUID, string parentUUID, string data)
        {
            bool result = false;
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
                var t = eventService.DoTreeUpdate(rootUUID);
            }
            return result;
        }

        public async Task<List<Model.NodeModel>> GetChildrenNode(string uuid)
        {
            return (await repository.GetChildrenNode(uuid)).Select(a => Model.NodeModel.FromDomain(a)).ToList();
        }

        public async Task<bool> UpdateNodeData(string uuid, string data)
        {
            var result = (await repository.UpdateNodeData(uuid, data));
            var t = eventService.DoNodeUpdate(uuid, data);
            return result;
        }

        public async Task<bool> DeleteNodeTree(string uuid)
        {
            var node = await GetNodeByUUID(uuid);
            var result = await repository.DeleteNodeTree(uuid);
            var t = eventService.DoTreeUpdate(node.root);
            return result;
        }

        public async Task<bool> MoveNode(string uuid, string newParent)
        {
            var node = await GetNodeByUUID(uuid);
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
                    await repository.MoveNode(uuid, newParent, NodeModel.IsBinaryleft(false));
                    result = true;
                }
                else
                {
                    await repository.MoveNode(uuid, newParent, NodeModel.IsBinaryleft(true));
                    result = true;
                }
            }
            else
            {
                await repository.MoveNode(uuid, newParent, NodeModel.IsBinaryleft(false));
                result = true;
            }
            if (result)
            {
                var t = eventService.DoTreeUpdate(root.uuid);
            }
            return result;
        }

        public async Task<bool> DeleteNode(string uuid)
        {
            var node = await GetNodeByUUID(uuid);
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
                        await repository.MoveNode(uuid, node.parent, NodeModel.IsBinaryleft(node.isBinaryleft));
                    }
                    await repository.DeleteNode(uuid);
                    result = true;
                }
            }
            else
            {
                await repository.DeleteNode(uuid);
                result = true;
            }
            if (result)
            {
                var t = eventService.DoTreeUpdate(root.uuid);
            }
            return result;
        }
        public async Task<TreeViewModel> GetNodeView(string uuid)
        {
            return TreeViewModel.FromDomain(await repository.GetNodeView(uuid));
        }
    }
}