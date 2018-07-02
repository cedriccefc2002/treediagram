using System;
using System.Threading.Tasks;
using Service = Server.lib.Service;
using Config = Server.lib.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using Ice;
using TreeDiagram;

namespace Server
{
    // 相容 IceBox
    public class IceClient
    {
        private Ice.ObjectAdapter adapter;
        private Ice.Communicator communicator;

        private ServerPrx _server;

        public ServerPrx server
        {
            get
            {
                return _server;
            }

        }

        private ServerEventPrx serverEvent;

        public async Task Connect(string[] args)
        {
            await Task.Delay(0);
            communicator = Ice.Util.initialize(ref args);
            Ice.Identity id = new Ice.Identity();
            var proxy = communicator.stringToProxy("Server:default -h localhost -p 10000");
            _server = ServerPrxHelper.checkedCast(proxy);
            adapter = communicator.createObjectAdapterWithEndpoints("TreeDiagramClient", "default -p 10008");
            adapter.activate();
            serverEvent = ServerEventPrxHelper.uncheckedCast(
                adapter.addWithUUID(
                    lib.Provider.serviceProvider.GetRequiredService<lib.IceBridge.ServerEvent>()));
            _server.initEvent(serverEvent);
        }
    }
}
