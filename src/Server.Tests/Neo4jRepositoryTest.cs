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
        public void CreateTree()
        {
            var repository = Provider.serviceProvider.GetService<Neo4jRepository>();
            Task.Run(async () =>
            {
                await repository.CreateTree(new TreeDomain()
                {
                    uuid = System.Guid.NewGuid().ToString(),
                    type = "Normal"
                });
            }).GetAwaiter().GetResult();
        }
    }
}
