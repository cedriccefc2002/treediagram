using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.lib.Config;
using Server.lib.Repository;
using Model = Server.lib.Service.Model;
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

        public async Task<List<Model.TreeModel>> readTree()
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return (await repository.ListAllTrees()).Select(a => Model.TreeModel.FromDomain(a)).ToList();
        }

        public async Task<bool> DeleteTree(string uuid)
        {
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return await repository.DeleteTree(uuid);
        }
    }
}