using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4j.Driver.V1;
using Server.lib.Config;

namespace Server.lib.Repository
{
    public class Neo4jRepository
    {
        private readonly ILogger<Neo4jRepository> _logger;
        private readonly IDriver _driver;

        public Neo4jRepository(ILogger<Neo4jRepository> logger)
        {
            _logger = logger;
            var config = Neo4jConfig.Config;
            _logger.LogInformation($"{config.uri}|{config.username}");
            _driver = GraphDatabase.Driver(config.uri, AuthTokens.Basic(config.username, config.password));
        }

        public async Task<bool> Status()
        {
            try
            {
                _logger.LogInformation("Status");
                await Task.Delay(0);
                using (var session = _driver.Session())
                {
                    // session.WriteTransaction((tx) =>
                    // {
                    //     var result = tx.Run(@"");
                    //     return result.Summary;
                    // });
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return true;
            }
        }
    }
}