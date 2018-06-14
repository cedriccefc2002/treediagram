using System;
using System.Threading.Tasks;
using Service = Server.lib.Service;
using Config = Server.lib.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Server
{
    public class Program
    {
        private async static Task StartIceServer()
        {
            var config = lib.ConfigProvider.IceAdapter;
            Console.WriteLine($"IceServer.name = \"{config.name}\"");
            Console.WriteLine($"IceServer.endpoints = \"{config.endpoints}\"");
            await Task.Delay(0);
            using (Ice.Communicator communicator = Ice.Util.initialize())
            {
                var adapter = communicator.createObjectAdapterWithEndpoints(config.name, config.endpoints);
                adapter.add(lib.Provider.serviceProvider.GetRequiredService<Service.ServerService>(), Ice.Util.stringToIdentity("Server"));
                adapter.activate();
                await Task.Factory.StartNew(() =>
                {
                    communicator.waitForShutdown();
                });
            }
        }
        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("StartIceServer");
                Task iceServerTask = StartIceServer();
                iceServerTask.Wait();
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
