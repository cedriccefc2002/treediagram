using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4j.Driver.V1;
using Server.lib.Config;

namespace Server.lib.Repository
{
    public class Neo4jRepository
    {
        private readonly ILogger<Neo4jRepository> logger;
        private readonly IDriver driver;

        public Neo4jRepository(ILogger<Neo4jRepository> logger)
        {
            this.logger = logger;
            var config = Neo4jConfig.Config;
            this.logger.LogInformation($"{config.uri}|{config.username}");
            driver = GraphDatabase.Driver(config.uri, AuthTokens.Basic(config.username, config.password), new Neo4j.Driver.V1.Config
            {
                // Failed after retried for 3 times in 5000 ms
                MaxTransactionRetryTime = TimeSpan.FromSeconds(5),
                ConnectionTimeout = TimeSpan.FromSeconds(1)
            });
        }

        public async Task<bool> Status()
        {
            try
            {

                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var summary = session.WriteTransaction((tx) =>
                    {
                        var result = tx.Run(@"RETURN datetime()");
                        return (result.Single())[0].As<string>();
                    });
                    logger.LogInformation(summary);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return true;
            }
        }
    }
}