using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.lib.Service
{
    public delegate void TreeListUpdateDelegate();
    public delegate void TreeUpdateDelegate(string uuid);
    public delegate void NodeUpdateDelegate(string uuid, string data);
    public class EventService
    {
        public event TreeListUpdateDelegate evtTreeListUpdate;
        public event TreeUpdateDelegate evtTreeUpdate;
        public event NodeUpdateDelegate evtNodeUpdate;
        private readonly ILogger<EventService> logger;
        public EventService(ILogger<EventService> logger)
        {
            this.logger = logger;
        }
        public async Task DoTreeListUpdate()
        {
            await Task.Delay(0);
            evtTreeListUpdate();
        }
        public async Task DoTreeUpdate(string uuid)
        {
            await Task.Delay(0);
            evtTreeUpdate(uuid);
        }
        public async Task DoNodeUpdate(string uuid, string data)
        {
            await Task.Delay(0);
            evtNodeUpdate(uuid, data);
        }
    }
}