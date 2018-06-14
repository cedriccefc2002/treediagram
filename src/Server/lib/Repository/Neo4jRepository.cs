using System;
using System.Collections.Generic;
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

        public async Task<bool> DeleteTree(string uuid)
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var count = session.WriteTransaction((tx) =>
                    {
                        var result = tx.Run(@"
                            MATCH (tree: Tree) WHERE tree.uuid = $uuid
                            DELETE tree
                            RETURN count(tree)
                        ", new { uuid });
                        return (result.Single())[0].As<uint>();
                    });
                    logger.LogInformation($"delete {count} {uuid}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return true;
            }
        }
        public async Task<bool> CreateTree(Domain.TreeDomain tree)
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var summary = session.WriteTransaction((tx) =>
                    {
                        var result = tx.Run(@"
                            CREATE (tree: Tree) 
                            SET 
                                tree.uuid = $uuid,
                                tree.type = $type
                            RETURN id(tree)
                        ", new
                        {
                            uuid = tree.uuid,
                            type = tree.type == Domain.TreeType.Binary ? "Binary" : "Normal"
                        });
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
        public async Task<List<Domain.TreeDomain>> ListAllTrees()
        {
            await Task.Delay(0);
            var result = new List<Domain.TreeDomain>();
            try
            {
                using (var session = driver.Session())
                {
                    var cursor = session.Run(@"
                        MATCH (tree: Tree) 
                        RETURN 
                            tree.uuid as uuid, 
                            tree.type as type
                    ");
                    foreach (var record in cursor)
                    {
                        result.Add(new Domain.TreeDomain()
                        {
                            uuid = record["uuid"].As<string>(),
                            type = record["type"].As<string>() == "Binary" ? Domain.TreeType.Binary : Domain.TreeType.Normal
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
            return result;
        }
        // private async Task RunSession()
        // {
        //     try
        //     {
        //         await Task.Delay(0);
        //         using (var session = driver.Session())
        //         {
        //             var result = await session.RunAsync("MATCH (movie:Movie) WHERE movie.title CONTAINS {title} RETURN movie", new { title = "q" });
        //             await result.ForEachAsync((record) =>
        //             {
        //                 var node = record["movie"].As<INode>();
        //             });
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.LogError(ex.Message);
        //     }
        // }
        public async Task<bool> Status()
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var cursor = session.Run(@"RETURN datetime()");
                    var result = cursor.Single()[0].As<string>(); ;
                    logger.LogInformation(result);
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