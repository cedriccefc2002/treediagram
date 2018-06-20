using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Server.lib;
using Server.lib.Service;
using Server.lib.Service.Model;
using System.Threading.Tasks;
using System;

namespace Server.Tests.lib.Repository
{
    [TestClass]
    public class ServerServiceTest
    {
        [TestMethod]
        public void TreeMethod()
        {
            var service = Provider.serviceProvider.GetService<ServerService>();
            Task.Run(async () =>
            {
                await service.Status();
                for (var i = 0; i < 100; i++)
                {
                    await service.createTree(new TreeModel()
                    {
                        uuid = System.Guid.NewGuid().ToString(),
                        type = TreeType.Normal
                    });
                }
                var trees = await service.ListAllTrees();
                foreach (var tree in trees)
                {
                    // await service.UpdateTreeType(tree.uuid, "Binary");
                    await service.DeleteTree(tree.uuid);
                }
            }).GetAwaiter().GetResult();
        }

        public void NodeMethod()
        {
            var service = Provider.serviceProvider.GetService<ServerService>();
            Task.Run(async () =>
            {
                await service.Status();
                for (var i = 0; i < 1; i++)
                {
                    await service.createTree(new TreeModel()
                    {
                        uuid = System.Guid.NewGuid().ToString(),
                        type = TreeType.Normal
                    });
                }
                var trees = await service.ListAllTrees();
                foreach (var tree in trees)
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var status = await service.CreateNode(tree.uuid, tree.uuid, $"node-{i}");
                        if (!status)
                        {
                            throw new Exception("CreateNodeError");
                        }
                    }
                    var children = await service.GetChildrenNode(tree.uuid);
                    foreach (var child in children)
                    {
                        await service.UpdateNodeData(child.uuid, $"node UpdateNodeData");
                        for (var i = 0; i < 10; i++)
                        {
                            var status = await service.CreateNode(tree.uuid, child.uuid, $"node-{i}");
                            if (!status)
                            {
                                throw new Exception("CreateNodeError");
                            }
                        }
                    }
                    if (children.Count > 4)
                    {
                        await service.DeleteNodeTree(children[0].uuid);
                        await service.MoveNode(children[1].uuid, children[2].uuid);
                        await service.DeleteNode(children[3].uuid);
                    }

                    var view = await service.GetNodeView(tree.uuid);
                    await service.DeleteTree(tree.uuid);
                }
            }).GetAwaiter().GetResult();
        }
    }
}
