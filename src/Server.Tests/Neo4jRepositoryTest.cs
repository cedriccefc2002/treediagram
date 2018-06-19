using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Server.lib;
using Server.lib.Repository;
using Server.lib.Repository.Domain;
using System.Threading.Tasks;
using System;

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
        [TestMethod]
        public void NodeMethod()
        {
            var repository = Provider.serviceProvider.GetService<Neo4jRepository>();
            Task.Run(async () =>
            {
                await repository.Status();
                for (var i = 0; i < 1; i++)
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
                    for (var i = 0; i < 10; i++)
                    {
                        var status = await repository.CreateNode(tree.uuid, tree.uuid, $"node-${i}");
                        if(!status){
                            throw new Exception("CreateNodeError");
                        }
                    }
                }
            }).GetAwaiter().GetResult();
        }
    }
}
