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
        private readonly Neo4jRepository repository;
        public ServerService(ILogger<ServerService> logger)
        {
            this.logger = logger;
            eventService = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
            repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
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

        public async Task<uint> GetChildrenCount(string uuid)
        {
            return await repository.GetChildrenCount(uuid);
        }
        public async Task<bool> CreateNode(string rootUUID, string parentUUID, string data)
        {
            var tree = await GetTreeByUUID(rootUUID);
            if (tree.type == TreeType.Binary)
            {
                if ((await GetChildrenCount(parentUUID)) > 2)
                {
                    return false;
                }
            }
            var result = await repository.CreateNode(rootUUID, parentUUID, data);
            var t = eventService.DoTreeUpdate(rootUUID);
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
            var result= await repository.DeleteNodeTree(uuid);
            var t = eventService.DoTreeUpdate(uuid);
            return result;
        }

        public async Task<bool> MoveNode(string uuid, string newParent)
        {
            var result = await repository.MoveNode(uuid, newParent);
            var t = eventService.DoTreeUpdate(uuid);
            return result;
        }

        public async Task<bool> DeleteNode(string uuid)
        {
            var result = await repository.DeleteNode(uuid);
            var t = eventService.DoTreeUpdate(uuid);
            return result;
        }
        public async Task<TreeViewModel> GetNodeView(string uuid)
        {
            return TreeViewModel.FromDomain(await repository.GetNodeView(uuid));
        }
    }
}