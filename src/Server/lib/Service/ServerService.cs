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
        public ServerService(ILogger<ServerService> logger)
        {
            this.logger = logger;
        }
        public async Task<bool> Status()
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return await repository.Status();
        }
        public async Task<bool> createTree(Model.TreeModel tree)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return await repository.CreateTree(tree.TreeDomain());
        }

        public async Task<List<Model.TreeModel>> ListAllTrees()
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return (await repository.ListAllTrees()).Select(a => Model.TreeModel.FromDomain(a)).ToList();
        }

        public async Task<bool> DeleteTree(string uuid)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return await repository.DeleteTree(uuid);
        }

        private async Task<TreeModel> GetTreeByUUID(string uuid)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return TreeModel.FromDomain(await repository.GetTreeByUUID(uuid));
        }

        private async Task<uint> GetChildrenCount(string uuid)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return await repository.GetChildrenCount(uuid);
        }
        public async Task<bool> CreateNode(string rootUUID, string parentUUID, string data)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            var tree = await GetTreeByUUID(rootUUID);
            if (tree.type == TreeType.Binary)
            {
                if ((await GetChildrenCount(parentUUID)) > 2)
                {
                    return false;
                }
            }
            return await repository.CreateNode(rootUUID, parentUUID, data);
        }

        public async Task<List<Model.NodeModel>> GetChildrenNode(string uuid)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return (await repository.GetChildrenNode(uuid)).Select(a => Model.NodeModel.FromDomain(a)).ToList();
        }

        public async Task<bool> UpdateNodeData(string uuid, string data)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return (await repository.UpdateNodeData(uuid, data));
        }

        public async Task<bool> DeleteNodeTree(string uuid)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return (await repository.DeleteNodeTree(uuid));
        }

        public async Task<bool> MoveNode(string uuid, string newParent)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return (await repository.MoveNode(uuid, newParent));
        }

        public async Task<bool> DeleteNode(string uuid)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return (await repository.DeleteNode(uuid));
        }
    }
}