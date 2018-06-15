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
                     tx.Run("CREATE CONSTRAINT ON (tree:Tree) ASSERT tree.uuid IS UNIQUE");
                     tx.Run("CREATE INDEX ON :Tree(type)");
                     tx.Run("CREATE CONSTRAINT ON (node:Node) ASSERT node.uuid IS UNIQUE");
                     tx.Run("CREATE INDEX ON :Node(root)");
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
        // 新增節點, 節點可儲存資料 (string)
        public async Task<bool> CreateNode(string root, string parent, string data = "")
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var uuid = Guid.NewGuid().ToString();
                    logger.LogInformation($"{uuid}|{data}|{root}|{parent}|Start");
                    var id = session.WriteTransaction((tx) =>
                    {
                        var query = @"
                            CREATE (node: Node) 
                            SET 
                                node.uuid = $uuid,
                                node.data = $data,
                                node.root = $root,
                                node.parent = $parent
                            RETURN id(node)
                        ";
                        var result = tx.Run(query, new { uuid, data, root });
                        tx.Run(@"
                            MATCH 
                                (p {uuid: $parent}), 
                                (n {uuid: $uuid})
                            CREATE (p)<-[:IsChild]-(n)
                        ");
                        return (result.Single())[0].As<string>();
                    });
                    logger.LogInformation($"{uuid}|{data}|{root}|{parent}|{id}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        // 編輯節點資料
        public async Task<bool> UpdateNodeData(string uuid, string data)
        {
            try
            {
                await Task.Delay(0);
                logger.LogInformation($"{uuid}|{data}");
                using (var session = driver.Session())
                {
                    session.WriteTransaction((tx) =>
                    {
                        var query = @"
                            MATCH 
                                (node: Node {uuid: $uuid})
                            SET 
                                node.data = $data
                        ";
                        tx.Run(query, new { uuid, data });
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        // 刪除子樹
        public async Task<bool> DeleteNodeTree(string uuid)
        {
            try
            {
                await Task.Delay(0);
                logger.LogInformation($"{uuid}");
                using (var session = driver.Session())
                {
                    session.WriteTransaction((tx) =>
                    {
                        var query = @"
                            MATCH ()<-[r1:IsChild*0..1]-(node: Node)<-[r2:IsChild*]-(children: Node)-[r3:IsChild*0..1]-() 
                            WHERE node.uuid = $uuid
                            FOREACH (x IN r1 | DELETE x)
                            FOREACH (x IN r2 | DELETE x)
                            FOREACH (x IN r3 | DELETE x)
                            DELETE node, children
                        ";
                        tx.Run(query, new { uuid });
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        // 刪除節點：子樹會保留
        public async Task<bool> DeleteNode(string uuid)
        {
            try
            {
                await Task.Delay(0);
                logger.LogInformation($"{uuid}");
                using (var session = driver.Session())
                {
                    session.WriteTransaction((tx) =>
                    {
                        var query = @"
                            MATCH (p)<-[r1:IsChild*0..1]-(node: Node)<-[r2:IsChild*0..1]-(children: Node) 
                            WHERE node.uuid = $uuid
                            FOREACH (child IN children | SET child.parent = node.parent)
                            FOREACH (child IN children | CREATE (p)<-[:IsChild]-(n))
                            FOREACH (x IN r1 | DELETE x)
                            FOREACH (x IN r2 | DELETE x)
                            DELETE node
                        ";
                        tx.Run(query, new { uuid });
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        // 搬移節點
        public async Task<bool> MoveNode(string uuid, string newParent)
        {
            try
            {
                await Task.Delay(0);
                logger.LogInformation($"{uuid} {newParent}");
                using (var session = driver.Session())
                {
                    session.WriteTransaction((tx) =>
                    {
                        tx.Run(@"
                            MATCH ()<-[r1:IsChild]-(node: Node{uuid: $uuid})
                            DELETE r1
                        ", new { uuid });
                        tx.Run(@"
                            MATCH 
                                (p {uuid: $newParent}), 
                                (n {uuid: $uuid})
                            CREATE (p)<-[:IsChild]-(n)
                        ", new { uuid, newParent });
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        // 檢視圖
        public async Task<bool> GetNodeView(string root)
        {
            try
            {
                await Task.Delay(0);
                logger.LogInformation($"{root}");
                using (var session = driver.Session())
                {
                    session.WriteTransaction((tx) =>
                    {
                        tx.Run(@"
                            MATCH p = (:Tree {uuid: $root})-[r*0..]->(x:Node)
                            WITH collect(DISTINCT x.uuid) as nodes, [r in collect(DISTINCT last(r)) | [startNode(r).uuid, endNode(r).uuid ]] as rels
                            RETURN size(nodes),size(rels), nodes, rels
                        ", new { root });
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
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
                    logger.LogInformation($"{count}|{uuid}");
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
                    var id = session.WriteTransaction((tx) =>
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
                    logger.LogInformation(id);
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
                        var uuid = record["uuid"].As<string>();
                        var type = record["type"].As<string>() == "Binary" ? Domain.TreeType.Binary : Domain.TreeType.Normal;
                        logger.LogInformation($"{uuid}|{type}");
                        result.Add(new Domain.TreeDomain()
                        {
                            uuid = uuid,
                            type = type
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

        public async Task<bool> Status()
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var cursor = session.Run(@"RETURN datetime()");
                    var result = cursor.Single()[0].As<string>(); ;
                    logger.LogInformation($"{result}");
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