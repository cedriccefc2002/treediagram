using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.lib.Service
{
    public delegate Task TreeListUpdateDelegate();

    public delegate Task TreeEditLockDelegate(string treeUUID, string clientUUID);
    public delegate Task TreeEditReleaseDelegate(string treeUUID);
    public delegate Task TreeEditFinishDelegate(string treeUUID)
    ;
    public delegate Task ClientConnectDelegate(string clientUUID);
    public delegate Task ClientDisConnectDelegate(string clientUUID);

    public delegate Task NodeUpdateDelegate(string uuid, string data, long timestamp);
    public class EventService
    {
        public event TreeListUpdateDelegate evtTreeListUpdate;

        public event ClientConnectDelegate evtClientConnect;
        public event ClientDisConnectDelegate evtClientDisConnect;

        public event TreeEditLockDelegate evtTreeEditLock;
        public event TreeEditReleaseDelegate evtTreeEditRelease;
        public event TreeEditFinishDelegate evtTreeEditFinish;

        public event NodeUpdateDelegate evtNodeUpdate;
        private readonly ILogger<EventService> logger;
        public EventService(ILogger<EventService> logger)
        {
            this.logger = logger;
        }
        public async Task DoTreeListUpdate()
        {
            await evtTreeListUpdate();
        }
        public async Task DoTreeEditLock(string treeUUID, string clientUUID)
        {
            await evtTreeEditLock(treeUUID, clientUUID);
        }
        public async Task DoTreeEditRelease(string treeUUID)
        {
            await evtTreeEditRelease(treeUUID);
        }
        public async Task DoTreeEditFinish(string treeUUID)
        {
            await evtTreeEditFinish(treeUUID);
        }

        public async Task DoClientConnect(string clientUUID)
        {
            await evtClientConnect(clientUUID);
        }
        public async Task DoClientDisConnect(string clientUUID)
        {
            await evtClientDisConnect(clientUUID);
        }

        public async Task DoNodeUpdate(string uuid, string data, long timestamp)
        {
            await evtNodeUpdate(uuid, data, timestamp);
        }
    }
}