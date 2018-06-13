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

        private async Task<string> SaveDataAsync(string message)
        {
            await Task.Delay(0);
            using (var session = _driver.Session())
            {
                // 使用`async function`+`WriteTransaction`可以運作
                var result = session.WriteTransaction((tx) =>
                {
                    var result1 = tx.Run(@"
                            CREATE (a:Greeting) SET a.message = $message 
                            RETURN a.message + ', from node ' + id(a)
                        ", new { message });
                    return (result1.Single())[0].As<string>();
                });
                return result;
                // 使用`async function`+`WriteTransactionAsync`會導致**Block**無回應
                // var result = await session.WriteTransactionAsync(async (tx) =>
                //     {
                //         await Task.Delay(0);
                //         var result1 = await tx.RunAsync(@"
                //             CREATE (a:Greeting) SET a.message = $message 
                //             RETURN a.message + ', from node ' + id(a)
                //         ", new { message });
                //         return (await result1.SingleAsync())[0].As<string>();
                //     });
                // return result;
            }
        }

        public override string saveData(string message, Ice.Current current)
        {
            var task = SaveDataAsync(message);
            task.Wait();
            return task.Result;

            // 使用`Task.Run`+`WriteTransactionAsync`可以運作
            // Task<string> task = Task<string>.Run(async () =>
            // {
            //     await Task.Delay(0);
            //     using (var session = _driver.Session())
            //     {
            //         var result = await session.WriteTransactionAsync(async (tx) =>
            //         {
            //             await Task.Delay(0);
            //             var result1 = await tx.RunAsync(@"
            //                 CREATE (a:Greeting) SET a.message = $message 
            //                 RETURN a.message + ', from node ' + id(a)
            //             ", new { message });
            //             return (await result1.SingleAsync())[0].As<string>();
            //         });
            //         return result;
            //     }
            // });
            // task.Wait();
            // return task.Result;

            // 使用`Task.Run`+`WriteTransaction`可以運作
            // Task<string> task = Task<string>.Run(async () =>
            // {
            //     await Task.Delay(0);
            //     using (var session = _driver.Session())
            //     {
            //         var result = session.WriteTransaction((tx) =>
            //         {
            //             var result1 = tx.Run(@"
            //                 CREATE (a:Greeting) SET a.message = $message 
            //                 RETURN a.message + ', from node ' + id(a)
            //             ", new { message });
            //             return (result1.Single())[0].As<string>();
            //         });
            //         return result;
            //     }
            // });
            // task.Wait();
            // return task.Result;
            // 直接執行
            // using (var session = _driver.Session())
            // {
            //     var result = session.WriteTransaction((tx) =>
            //         {
            //             var result1 = tx.Run(@"
            //                 CREATE (a:Greeting) SET a.message = $message 
            //                 RETURN a.message + ', from node ' + id(a)
            //             ", new { message });
            //             return (result1.Single())[0].As<string>();
            //         });
            //     return result;
            // }
        }
    }
    public class Program
    {
        private readonly static IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "12369874"));
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
        // public static async Task<string> SaveData(string message)
        // {
        //     await Task.Delay(0);
        //     var session = _driver.Session();
        //     return await session.WriteTransactionAsync(async (tx) =>
        //     {
        //         await Task.Delay(0);
        //         // Console.WriteLine("WriteTransactionAsync");
        //         // var result1 = await tx.RunAsync("CREATE (a:Greeting) " +
        //         //                     "SET a.message = $message " +
        //         //                     "RETURN a.message + ', from node ' + id(a)",
        //         //     new { message });
        //         // Console.WriteLine(result1);
        //         // var r = await result1.SingleAsync();
        //         // Console.WriteLine(r);
        //         return "SaveData";
        //     });
        // }
        public static int Main(string[] args)
        {
            try
            {
                Task iceServerTask = StartIceServer();
                // SaveData("s").Wait();
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
