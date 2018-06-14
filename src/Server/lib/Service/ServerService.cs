using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.lib.Config;
using Server.lib.Repository;
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
            await Task.Delay(0);
            var repository = lib.Provider.serviceProvider.GetRequiredService<Neo4jRepository>();
            return await repository.Status();
        }
    }
}