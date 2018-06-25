using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4j.Driver.V1;
using Server.lib.Config;
using Server.lib.Repository.Domain;

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
                        var parms = new { uuid, data, root, parent };
                        var query = @"
                            CREATE (node: Node) 
                            SET 
                                node.uuid = $uuid,
                                node.data = $data,
                                node.root = $root,
                                node.parent = $parent
                            RETURN id(node)
                        ";
                        var result = tx.Run(query, parms);
                        tx.Run(@"
                            MATCH 
                                (p {uuid: $parent}), 
                                (n {uuid: $uuid})
                            CREATE (p)<-[:IsChild]-(n)
                        ", parms);
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
                            MATCH ()<-[r1:IsChild*0..1]-(node)<-[r2:IsChild*]-(children: Node)-[r3:IsChild*0..1]-() 
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
        // 讀取子節點的數目
        public async Task<uint> GetChildrenCount(string uuid)
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var cursor = session.Run(@"
                        MATCH (p)<-[r:IsChild*1]-()
                        WHERE p.uuid = $uuid 
                        WITH collect(r) AS rs
                        RETURN size(rs)
                    ", new { uuid });
                    var result = cursor.Single()[0].As<uint>(); ;
                    logger.LogInformation($"{uuid} {result}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw ex;
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
                            MATCH
                                (p)<-[r1:IsChild*0..1]-(node: Node)<-[r2:IsChild*0..1]-(children: Node)
                            WHERE node.uuid = $uuid
                            WITH
                                collect(children) AS childrens,
                                head(collect(node)) AS self,
                                head(collect(p)) AS parent,
                                r1 AS R1,
                                r2 AS R2
                            FOREACH (child IN childrens | CREATE (parent)<-[:IsChild]-(child))
                            FOREACH (child IN childrens | SET child.parent = self.parent)
                            FOREACH (x IN R1 | DELETE x)
                            FOREACH (x IN R2 | DELETE x)
                            DETACH DELETE self
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
        public async Task<TreeViewDomain> GetNodeView(string root)
        {
            try
            {
                await Task.Delay(0);
                logger.LogInformation($"{root}");
                using (var session = driver.Session())
                {
                    var result = session.ReadTransaction((tx) =>
                    {
                        return tx.Run(@"
                            MATCH p = (:Tree {uuid: $root})<-[r*0..]-(x:Node)
                            WITH
                                collect(DISTINCT x) as nodes, 
                                [
                                    r in collect(DISTINCT last(r)) | 
                                    { 
                                        parentUUID: endNode(r).uuid, 
                                        childUUID: startNode(r).uuid 
                                    }
                                ] as rels
                            RETURN size(nodes) AS nodesCount,size(rels) AS relsCount, nodes, rels
                        ", new { root });
                    }).Peek();
                    var nodesCount = result["nodesCount"].As<uint>();
                    var relsCount = result["relsCount"].As<uint>();
                    var nodeNode = result["nodes"].As<IList<INode>>();
                    var relNode = result["rels"].As<IList<IList<KeyValuePair<string, Object>>>>();
                    logger.LogInformation($"{nodesCount}|{relsCount}");
                    var nodes = new List<NodeDomain>();
                    var rels = new List<NodeRelationshipDomain>();
                    foreach (var c in nodeNode)
                    {
                        nodes.Add(new NodeDomain()
                        {
                            uuid = c.Properties["uuid"].As<string>(),
                            data = c.Properties["data"].As<string>(),
                            root = c.Properties["root"].As<string>(),
                            parent = c.Properties["parent"].As<string>(),
                        });
                    }
                    foreach (var c in relNode)
                    {
                        rels.Add(new NodeRelationshipDomain()
                        {
                            parentUUID = c[0].Value.As<string>(),
                            childUUID = c[1].Value.As<string>(),
                        });
                    }
                    return new TreeViewDomain()
                    {
                        uuid = root,
                        nodes = nodes,
                        rels = rels
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw ex;
            }
        }
        public async Task<IList<NodeDomain>> GetChildrenNode(string parent)
        {
            await Task.Delay(0);
            var result = new List<NodeDomain>();
            try
            {
                logger.LogInformation($"{parent}");
                using (var session = driver.Session())
                {
                    var cursor = session.ReadTransaction((tx) =>
                    {
                        return tx.Run(@"
                            MATCH (node {parent: $parent})
                            RETURN 
                                node.uuid AS uuid,
                                node.data AS data,
                                node.root AS root
                        ", new { parent });
                    });
                    foreach (var record in cursor)
                    {
                        var uuid = record["uuid"].As<string>();
                        var data = record["data"].As<string>();
                        var root = record["root"].As<string>();
                        logger.LogInformation($"{uuid}|{data}|{root}");
                        result.Add(new NodeDomain()
                        {
                            uuid = uuid,
                            data = data,
                            root = root,
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
        #region Tree
        public async Task<bool> DeleteTree(string uuid)
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    session.WriteTransaction((tx) =>
                    {
                        // var query = @"
                        //     MATCH (tree: Tree { uuid: $uuid})
                        //     OPTIONAL MATCH (tree)-[r]-() 
                        //     DELETE tree, r
                        //     RETURN count(tree)
                        // ";
                        // Starting in Neo4j 2.3.x
                        var parms = new { uuid };
                        var nodeCount = tx.Run(@"
                            MATCH (node: Node { root: $uuid})
                            DETACH DELETE node
                            RETURN count(node)
                        ", parms).Single()[0].As<uint>();
                        var treeCount = tx.Run(@"
                            MATCH (tree: Tree { uuid: $uuid})
                            DETACH DELETE tree
                            RETURN count(tree)
                        ", parms).Single()[0].As<uint>();
                        logger.LogInformation($"{nodeCount}|{treeCount}|{uuid}");
                        tx.Success();
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
        public async Task<bool> CreateTree(TreeDomain tree)
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
                                tree.type = $type,
                                tree.lastUpdateDate = datetime.realtime()
                            RETURN id(tree)
                        ", new
                        {
                            uuid = tree.uuid,
                            type = tree.type,
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
        public async Task<TreeDomain> GetTreeByUUID(string uuid)
        {
            await Task.Delay(0);
            try
            {
                logger.LogInformation($"{uuid}");
                using (var session = driver.Session())
                {
                    var result = session.Run(@"
                        MATCH (tree: Tree) 
                        Where tree.uuid = $uuid
                        RETURN 
                            tree.uuid as uuid, 
                            tree.type as type,
                            tree.lastUpdateDate as lastUpdateDate
                    ", new { uuid }).Peek();
                    return new Domain.TreeDomain()
                    {
                        uuid = result["uuid"].As<string>(),
                        type = result["type"].As<string>(),
                        lastUpdateDate = result["lastUpdateDate"].As<string>(),
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw ex;
            }
        }
        public async Task<IList<TreeDomain>> ListAllTrees()
        {
            await Task.Delay(0);
            var result = new List<TreeDomain>();
            try
            {
                using (var session = driver.Session())
                {
                    var cursor = session.Run(@"
                        MATCH (tree: Tree) 
                        RETURN 
                            tree.uuid as uuid, 
                            tree.type as type,
                            tree.lastUpdateDate as lastUpdateDate
                    ");
                    foreach (var record in cursor)
                    {
                        var uuid = record["uuid"].As<string>();
                        var type = record["type"].As<string>();
                        var lastUpdateDate = record["lastUpdateDate"].As<string>();
                        logger.LogInformation($"{uuid}|{type}");
                        result.Add(new Domain.TreeDomain()
                        {
                            uuid = uuid,
                            type = type,
                            lastUpdateDate = lastUpdateDate,
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
        public async Task<bool> UpdateTreeType(string uuid, string type)
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var id = session.WriteTransaction((tx) =>
                    {
                        var result = tx.Run(@"
                            MATCH (tree: Tree) 
                            WHERE tree.uuid = $uuid
                            SET tree.type = $type
                            RETURN id(tree)
                        ", new
                        {
                            uuid,
                            type,
                        });
                        return (result.Single())[0].As<string>();
                    });
                    logger.LogInformation($"{uuid}|{id}|{type}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        public async Task<bool> UpdateTreeDateTime(string uuid)
        {
            try
            {
                await Task.Delay(0);
                using (var session = driver.Session())
                {
                    var id = session.WriteTransaction((tx) =>
                    {
                        var result = tx.Run(@"
                            MATCH (tree: Tree) 
                            WHERE tree.uuid = $uuid
                            SET tree.lastUpdateDate = datetime.realtime()
                            RETURN id(tree)
                        ", new
                        {
                            uuid,
                        });
                        return (result.Single())[0].As<string>();
                    });
                    logger.LogInformation($"{uuid}|{id}");
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
                return false;
            }
        }
    }
}