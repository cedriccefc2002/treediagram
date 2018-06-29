using System;
using System.Threading.Tasks;
using Service = Server.lib.Service;
using Config = Server.lib.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using Server.lib.gRPCBridge;

namespace Server
{
    public class gRPCService
    {
        public async static Task StartService(string[] args)
        {
            await Task.Delay(0);
            var provider = lib.Provider.serviceProvider;
            var port = 3000;
            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = {
                    TreeDiagram.GridLogService.BindService(provider.GetRequiredService<lib.gRPCBridge.Server>())
                },
                Ports = { new Grpc.Core.ServerPort("localhost", port, Grpc.Core.ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine("gRPC server listening on port " + port);
            while (true)
            {
                await Task.Delay(1000);
            }
            // await server.ShutdownAsync();
        }
    }
}