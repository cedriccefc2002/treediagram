using System;
using System.Threading.Tasks;
using System.Linq;
using Neo4j.Driver.V1;
using System.Threading;

namespace Server
{
    class Node : TreeDiagram.NodeDisp_
    {
        private readonly IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "12369874"));

        private async Task<string> SaveData(string message)
        {
            using (var session = _driver.Session())
            {
                var result = await session.WriteTransactionAsync(async (tx) =>
                {
                    var result1 = await tx.RunAsync("CREATE (a:Greeting) " +
                                        "SET a.message = $message " +
                                        "RETURN a.message + ', from node ' + id(a)",
                        new { message });
                    return (await result1.SingleAsync())[0].As<string>();
                });
                return result;
            }
        }

        public override string saveData(string message, Ice.Current current)
        {
            Console.WriteLine(message);
            using (var session = _driver.Session())
            {
                var result = session.WriteTransaction((tx) =>
                {
                    var result1 = tx.Run("CREATE (a:Greeting) " +
                                        "SET a.message = $message " +
                                        "RETURN a.message + ', from node ' + id(a)",
                        new { message });
                    return (result1.Single())[0].As<string>();
                });
                return result;
            }
        }
    }
    public class Program
    {
        private async static Task StartIceServer()
        {
            await Task.Delay(0);
            Console.WriteLine("IceServer Start");
            using (Ice.Communicator communicator = Ice.Util.initialize())
            {
                var adapter = communicator.createObjectAdapterWithEndpoints("TreeDiagramAdapter", "default -h localhost -p 10000");
                adapter.add(new Node(), Ice.Util.stringToIdentity("Node"));
                adapter.activate();
                await Task.Factory.StartNew(() =>
                {
                    communicator.waitForShutdown();
                    Console.WriteLine("IceServer Stop");
                });
            }
        }
        public static int Main(string[] args)
        {
            try
            {
                Task iceServerTask = StartIceServer();
                Console.WriteLine("按下任意鍵停止");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
            return 0;
        }
    }
}
