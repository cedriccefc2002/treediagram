import { EventEmitter } from "events";

import { Ice, Glacier2 } from "ice";
import { v4 as uuidv4 } from "uuid";

import { TreeDiagram } from "../Ice/Server";

import { TreeDiagram as TreeDiagram_Tree } from "../Ice/Tree";

export type TreeType = TreeDiagram_Tree.TreeType;

export class Tree extends TreeDiagram_Tree.Tree {
    public constructor(type: TreeType) {
        super();
        this.uuid = uuidv4();
        this.type = type
    }
}

export class ClientEvent extends TreeDiagram.ServerEvent {
    public event: EventEmitter = new EventEmitter();
    public static eventTreeListUpdate = Symbol();
    public static eventTreeUpdate = Symbol();
    public static eventNodeUpdate = Symbol();

    public TreeListUpdate() {
        this.event.emit(ClientEvent.eventTreeListUpdate);
    }

    public TreeUpdate(uuid: string) {
        this.event.emit(ClientEvent.eventTreeUpdate, uuid);
    }

    public NodeUpdate(uuid: string, data: string) {
        this.event.emit(ClientEvent.eventNodeUpdate, uuid, data);
    }
}

async function setImmediateAsync() {
    return new Promise((resulve, reject) => {
        setImmediate(resulve);
    });
}
async function setTimeoutAsync(timeout: number) {
    return new Promise((resulve, reject) => {
        setTimeout(resulve, timeout);
    });
}

export class Proxy {
    private static singleProxy: Proxy | null = null;
    public event: ClientEvent = new ClientEvent();
    private server: TreeDiagram.ServerPrx | null = null;
    private communicator: Ice.Communicator | null = null;
    private router: Glacier2.RouterPrx | null = null;
    private constructor() { }

    public async init() {
        await this.getServer();
    }

    public async server_listAllTrees() {
        return (await this.getServer()).listAllTrees();
    }
    public async server_createTree(type: TreeType) {
        return (await this.getServer()).createTree(new Tree(type));
    }
    public async server_deleteTree(uuid: string) {
        return (await this.getServer()).deleteTree(uuid);
    }

    private GetIceCommunicator() {
        if (this.communicator === null) {
            this.communicator = Ice.initialize([
                "--Ice.Default.Router=PublicRouter/router:tcp -h 127.0.0.1 -p 10001",
                "--Ice.ACM.Client.Heartbeat=3",
            ]);
        }
        return this.communicator;
    }
    private async GetIceRouter() {
        if (this.router === null) {
            console.log("Create IceRouter");
            const communicator = this.GetIceCommunicator();
            this.router = await Glacier2.RouterPrx.checkedCast(communicator.getDefaultRouter());
            if (this.router === null) {
                await this.disconnect();
                throw new Error("Router Is null")
            } else {
                const connection = this.router.ice_getCachedConnection();
                if (connection === null) {
                    await this.disconnect();
                    throw new Error("connection Is null")
                } else {
                    connection.setCloseCallback(async () => {
                        console.log("Connection close");
                        await this.disconnect();
                    });
                }
            }
        }
        return this.router;
    }

    private async getServer() {
        if (this.server === null) {
            console.log("Create IceServer");
            const communicator = this.GetIceCommunicator();
            const router = await this.GetIceRouter();
            const session = await router.createSession("username", "password");
            const timeout = await router.getSessionTimeout();
            const category = await router.getCategoryForClient();
            const adapter = await communicator.createObjectAdapterWithRouter("", router);
            const proxy = communicator.stringToProxy("Server:tcp -h 127.0.0.1 -p 10000");
            const serverEvent = TreeDiagram.ServerEventPrx.uncheckedCast(adapter.add(this.event, new Ice.Identity("serverEvent", category)));
            this.server = await TreeDiagram.ServerPrx.checkedCast(proxy);
            if (this.server !== null && serverEvent !== null) {
                await this.server.initEvent(serverEvent);
            }
            else {
                await this.disconnect();
                throw new Error("Invalid server")
            }
        }
        return this.server;
    }
    private async disconnect() {
        this.server = null;
        this.router = null;
        if (this.communicator !== null) {
            this.communicator.destroy();
            this.communicator = null;
        }
    }
    public static async GetProxy() {
        if (Proxy.singleProxy === null) {
            Proxy.singleProxy = new Proxy();
        }
        return Proxy.singleProxy;
    }
}

if (require.main === module) {
    (async function () {
        const proxy = await Proxy.GetProxy();
        proxy.event.event.on(ClientEvent.eventTreeListUpdate, async () => {
            console.log("eventTreeListUpdate");
        });
        await proxy.init();
        while (true) {
            await setTimeoutAsync(10000);
            try {
                for (let i = 0; i < 10; i++) {
                    await proxy.server_createTree(TreeDiagram_Tree.TreeType.Binary);
                }
                const trees = await proxy.server_listAllTrees();
                console.dir(trees);
                for(const tree of trees) {
                    await proxy.server_deleteTree(tree.uuid);
                }
            } catch (error) {
                console.error(error);
            }
        }
    })();
}