// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using Neo4j.Driver.V1;

// namespace Server.lib.Service
// {
// class NodeService : TreeDiagram.NodeDisp_
// {
//     private readonly ILogger<NodeService> _logger;
//     private readonly IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "12369874"));

//     public NodeService(ILogger<NodeService> logger)
//     {
//         _logger = logger;
//     }
//     private async Task<string> saveDataAsync(string message)
//     {
//         await Task.Delay(0);
//         using (var session = _driver.Session())
//         {
//             // 使用`async function`+`WriteTransaction`可以運作
//             var result = session.WriteTransaction((tx) =>
//             {
//                 var result1 = tx.Run(@"
//                         CREATE (a:Greeting) SET a.message = $message 
//                         RETURN a.message + ', from node ' + id(a)
//                     ", new { message });
//                 return (result1.Single())[0].As<string>();
//             });
//             return result;
//             // 使用`async function`+`WriteTransactionAsync`會導致**Block**無回應
//             // var result = await session.WriteTransactionAsync(async (tx) =>
//             //     {
//             //         await Task.Delay(0);
//             //         var result1 = await tx.RunAsync(@"
//             //             CREATE (a:Greeting) SET a.message = $message 
//             //             RETURN a.message + ', from node ' + id(a)
//             //         ", new { message });
//             //         return (await result1.SingleAsync())[0].As<string>();
//             //     });
//             // return result;
//         }
//     }

//     public override string saveData(string message, Ice.Current current)
//     {
//         _logger.LogInformation(message);
//         var task = saveDataAsync(message);
//         // task.Wait(); 不需要 task.Result 會 blocks until the result is available
//         // Console.WriteLine("SaveDataAsync");
//         return task.Result;

//         // 使用`Task.Run`+`WriteTransactionAsync`可以運作
//         // Task<string> task = Task<string>.Run(async () =>
//         // {
//         //     await Task.Delay(0);
//         //     using (var session = _driver.Session())
//         //     {
//         //         var result = await session.WriteTransactionAsync(async (tx) =>
//         //         {
//         //             await Task.Delay(0);
//         //             var result1 = await tx.RunAsync(@"
//         //                 CREATE (a:Greeting) SET a.message = $message 
//         //                 RETURN a.message + ', from node ' + id(a)
//         //             ", new { message });
//         //             return (await result1.SingleAsync())[0].As<string>();
//         //         });
//         //         return result;
//         //     }
//         // });
//         // task.Wait();
//         // return task.Result;

//         // 使用`Task.Run`+`WriteTransaction`可以運作
//         // Task<string> task = Task<string>.Run(async () =>
//         // {
//         //     await Task.Delay(0);
//         //     using (var session = _driver.Session())
//         //     {
//         //         var result = session.WriteTransaction((tx) =>
//         //         {
//         //             var result1 = tx.Run(@"
//         //                 CREATE (a:Greeting) SET a.message = $message 
//         //                 RETURN a.message + ', from node ' + id(a)
//         //             ", new { message });
//         //             return (result1.Single())[0].As<string>();
//         //         });
//         //         return result;
//         //     }
//         // });
//         // task.Wait();
//         // return task.Result;
//         // 直接執行
//         // using (var session = _driver.Session())
//         // {
//         //     var result = session.WriteTransaction((tx) =>
//         //         {
//         //             var result1 = tx.Run(@"
//         //                 CREATE (a:Greeting) SET a.message = $message 
//         //                 RETURN a.message + ', from node ' + id(a)
//         //             ", new { message });
//         //             return (result1.Single())[0].As<string>();
//         //         });
//         //     return result;
//         // }
//     }
// }
// }