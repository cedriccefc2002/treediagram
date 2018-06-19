using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Server.lib;
using Server.lib.Repository;
using Server.lib.Repository.Domain;
using System.Threading.Tasks;

namespace Server.Tests.lib.Repository
{
    [TestClass]
    public class Neo4jRepositoryTest
    {
        [TestMethod]
        public void TreeMethod()
        {
            var repository = Provider.serviceProvider.GetService<Neo4jRepository>();
            Task.Run(async () =>
            {
                await repository.Status();
                for (var i = 0; i < 100; i++)
                {
                    await repository.CreateTree(new TreeDomain()
                    {
                        uuid = System.Guid.NewGuid().ToString(),
                        type = "Normal"
                    });
                }
                var trees = await repository.ListAllTrees();
                foreach (var tree in trees)
                {
                    await repository.GetTreeByUUID(tree.uuid);
                    await repository.UpdateTreeDateTime(tree.uuid);
                    await repository.UpdateTreeType(tree.uuid, "Binary");
                    await repository.DeleteTree(tree.uuid);
                }
            }).GetAwaiter().GetResult();
        }
    }
}
