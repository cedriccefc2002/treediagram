using System;
using System.Threading.Tasks;
using System.Linq;
using Neo4j.Driver.V1;
using System.Threading;
using Service = Server.lib.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Server
{

    public class Program
    {
        private async static Task StartIceServer(IServiceProvider serviceProvider)
        {
            await Task.Delay(0);
            using (Ice.Communicator communicator = Ice.Util.initialize())
            {
                var adapter = communicator.createObjectAdapterWithEndpoints("TreeDiagramAdapter", "default -h localhost -p 10000");
                adapter.add(serviceProvider.GetRequiredService<Service.NodeService>(), Ice.Util.stringToIdentity("Node"));
                adapter.add(serviceProvider.GetRequiredService<Service.ServerService>(), Ice.Util.stringToIdentity("Server"));
                adapter.activate();
                await Task.Factory.StartNew(() =>
                {
                    communicator.waitForShutdown();
                    // Console.WriteLine("IceServer Stop");
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
                IServiceProvider serviceProvider = Service.Provider.Init();
                Task iceServerTask = StartIceServer(serviceProvider);
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
