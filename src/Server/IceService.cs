using System;
using System.Threading.Tasks;
using Service = Server.lib.Service;
using Config = Server.lib.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using Ice;

namespace Server
{
    // 相容 IceBox
    public class IceService : IceBox.Service, IDisposable
    {
        private Ice.ObjectAdapter adapter;
        private IceClient client = new IceClient();
        public async static Task StartService(string[] args)
        {
            Console.WriteLine("StartIceServer");
            await Task.Delay(0);
            var config = Config.IceAdapterConfig.Config;
            Console.WriteLine($"IceServer.name = \"{config.name}\"");
            using (Ice.Communicator communicator = Ice.Util.initialize(ref args))
            using (IceService iceService = new IceService())
            {
                iceService.start(config.name, communicator, args);
                Console.WriteLine("Ice server listening");
                await iceService.client.Connect(args);
                await Task.Factory.StartNew(() =>
                {
                    communicator.waitForShutdown();
                });
            }
        }

        private void AddService(ref Ice.ObjectAdapter adapter)
        {
            var provider = lib.Provider.serviceProvider;
            adapter.add(provider.GetRequiredService<lib.IceBridge.Server>(), Ice.Util.stringToIdentity("Server"));
        }

        public void start(string name, Communicator communicator, string[] args)
        {
            var config = Config.IceAdapterConfig.Config;
            Console.WriteLine($"IceServer.endpoints = \"{config.endpoints}\"");
            adapter = communicator.createObjectAdapterWithEndpoints(name, config.endpoints);
            AddService(ref adapter);
            adapter.activate();
        }

        public void stop()
        {
            adapter.deactivate();
        }

        public void Dispose()
        {
            stop();
        }
    }
}
