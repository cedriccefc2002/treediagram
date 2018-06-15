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
            this.CreateIndex(ref driver);
        }

        protected void CreateIndex(ref IDriver driver)
        {
            using (var session = driver.Session())
            {
                logger.LogInformation($"Create Index");
                session.WriteTransaction((tx) =>
                 {
                     tx.Run(" CREATE INDEX ON :Tree(uuid)");
                     tx.Run(" CREATE INDEX ON :Tree(type)");
                     tx.Run(" CREATE INDEX ON :Node(uuid)");
                     tx.Run(" CREATE INDEX ON :Node(root)");
                     tx.Success();
                 });
            }
        }

        /*
        MATCH 
            (u:Tree {uuid:'4b9f5d70-6fb1-11e8-a86b-d1aef71b8da5'}), 
            (r:Tree {uuid:'4a7cd670-6fb1-11e8-a86b-d1aef71b8da5'})
        CREATE (u)-[:Root]->(r)
        
        https://gist.github.com/jexp/5c1092933781ec4ea2a3
        CREATE (r:root)
        FOREACH (i IN range(1,5)|
            CREATE (r)-[:PARENT]->(c:child { id:i }));

        MATCH (c:child)
            FOREACH (j IN range(1,5)|
                CREATE (c)-[:PARENT]->(:child { id:c.id*10+j }));

        match (n)-[r]-()
            return count(distinct n) as nodes, count(distinct r) as rels
        
        MATCH p = (:root)-[r*]->(x)
        WITH collect(DISTINCT id(x)) as nodes, [r in collect(distinct last(r)) | [id(startNode(r)),id(endNode(r))]] as rels
        RETURN size(nodes),size(rels), nodes, rels

        MATCH p = (:root)-[r*0..]->(x)
        WITH collect(DISTINCT id(x)) as nodes, [r in collect(distinct last(r)) | [id(startNode(r)),id(endNode(r))]] as rels
        RETURN size(nodes),size(rels), nodes, rels
        
        https://stackoverflow.com/questions/27989740/deleting-a-tree-of-nodes-using-cypher
        
        MATCH ()<-[r1*0..1]-(a)<-[rels*]-(t)-[r2*0..1]-()
        WHERE ID(a)=135
        FOREACH (x IN r1 | DELETE x)
        FOREACH (x IN r2 | DELETE x)
        FOREACH (x IN rels | DELETE x)
        DELETE a, t
         */
        #region Node
        public async Task<bool> CreateNode(string root, string data = "")
        {
            await Task.Delay(0);
            using (var session = driver.Session())
            {
                var uuid = Guid.NewGuid().ToString();
                var id = session.WriteTransaction((tx) =>
                {
                    var query = @"
                            CREATE (node: Node) 
                            SET 
                                node.uuid = $uuid,
                                node.data = $data
                                node.root = $root
                            RETURN id(node)
                        ";
                    var result = tx.Run(query, new { uuid });
                    return (result.Single())[0].As<string>();
                });
                logger.LogInformation($"delete {id} {uuid} {data} {root}");
                return true;
            }
        }
        #endregion 
        #region Tree
        public async Task<bool> DeleteTree(string uuid)
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var count = session.WriteTransaction((tx) =>
                    {
                        // var query = @"
                        //     MATCH (tree: Tree { uuid: $uuid})
                        //     OPTIONAL MATCH (tree)-[r]-() 
                        //     DELETE tree, r
                        //     RETURN count(tree)
                        // ";
                        // Starting in Neo4j 2.3.x
                        var query = @"
                            MATCH (tree: Tree { uuid: $uuid})
                            DETACH DELETE tree
                            RETURN count(tree)
                        ";
                        var result = tx.Run(query, new { uuid });
                        return (result.Single())[0].As<uint>();
                    });
                    logger.LogInformation($"delete {count} {uuid}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
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
                return false;
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
        #endregion

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