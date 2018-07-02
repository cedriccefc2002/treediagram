using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ice;
using TreeDiagram;
using Service = Server.lib.Service;
using Model = Server.lib.Service.Model;
using Domain = Server.lib.Repository.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System;
using System.Collections.Generic;
namespace Server.lib.IceBridge
{
    public class ServerEvent : TreeDiagram.ServerEventDisp_
    {
        private readonly ILogger<Server> logger;
        private readonly Service.EventService eventService;

        public ServerEvent(ILogger<Server> logger)
        {
            this.logger = logger;
            this.eventService = lib.Provider.serviceProvider.GetRequiredService<Service.EventService>();
        }

        public override void TreeListUpdate(Current context)
        {
            this.logger.LogInformation("");
        }
        public override void TreeEditLock(string treeUUID, string clientUUID, Current context)
        {
            this.logger.LogInformation($"{treeUUID} {clientUUID}");
        }
        public override void TreeEditRelease(string treeUUID, Current context)
        {
            this.logger.LogInformation(treeUUID);
        }
        public override void TreeEditFinish(string treeUUID, Current context)
        {
            this.logger.LogInformation(treeUUID);
        }
        public override void NodeUpdate(string uuid, string data, long timestamp, Current context)
        {
            this.logger.LogInformation($"{uuid}:{data}:{timestamp.ToString()}");
        }

        
    }
}