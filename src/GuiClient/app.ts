import * as path from "path";
// tslint:disable-next-line:no-var-requires
// require("module").globalPaths.push(path.join(process.cwd(), "Ice"));
// require("module").globalPaths.push(path.join(__dirname, "Ice"));

module.paths.push(path.join(__dirname, "Ice"));

// import * as uuid from "uuid";
import { Ice, Glacier2 } from "ice";

import { TreeDiagram } from "./Ice/Server";

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

let communicator: Ice.Communicator;

class ServerEventI extends TreeDiagram.ServerEvent {
    public TreeListUpdate() {
        console.log("TreeListUpdate")
    }
}

async function create() {
    try {
        communicator = Ice.initialize([
            "--Ice.Default.Router=PublicRouter/router:tcp -h 127.0.0.1 -p 10001",
            "--Ice.ACM.Client.Heartbeat=3",
            // "--Ice.RetryIntervals=-1",
            // "--Ice.ACM.Client=0"
        ]);
        const router = await Glacier2.RouterPrx.checkedCast(communicator.getDefaultRouter());
        // console.log(router);
        if (router) {
            const session = await router.createSession("username", "password");
            // console.log(session);
            const [timeout, category, adapter] = await Promise.all([
                router.getSessionTimeout(),
                router.getCategoryForClient(),
                communicator.createObjectAdapterWithRouter("", router)],
            );
            // console.log(timeout);
            const proxy = communicator.stringToProxy("Server:tcp -h 127.0.0.1 -p 10000");
            const serverEvent = TreeDiagram.ServerEventPrx.uncheckedCast(adapter.add(new ServerEventI(), new Ice.Identity("serverEvent", category)));
            const server = await TreeDiagram.ServerPrx.checkedCast(proxy);
            if (server) {
                await server.initEvent(serverEvent);
            }
        }

    } catch (error) {
        // tslint:disable-next-line:no-console
        console.error(error);
    }
}
// const Ice = require("ice").Ice;
// const Glacier2 = require("ice").Glacier2;
// const TreeDiagram = require("./Ice/Server").TreeDiagram;
// async function setImmediateAsync() {
//     return new Promise((resulve, reject) => {
//         setImmediate(resulve);
//     })
// } async function setTimeoutAsync(timeout) {
//     return new Promise((resulve, reject) => {
//         setTimeout(resulve, timeout);
//     })
// }
// let communicator;
// class ServerEventI extends TreeDiagram.ServerEvent {
//     TreeListUpdate(current) {
//         console.log("TreeListUpdate")
//         console.dir(current)
//     }
// }

// async function create() {
//     try {
//         communicator = Ice.initialize([
//             "--Ice.Default.Router=PublicRouter/router:tcp -h 127.0.0.1 -p 10001",
//             "--Ice.ACM.Client.Heartbeat=3"
//             // "--Ice.RetryIntervals=-1",
//             // "--Ice.ACM.Client=0"
//         ]);
//         // const base = ic.stringToProxy("Server:default -h localhost -p 10000");
//         let router = await Glacier2.RouterPrx.checkedCast(communicator.getDefaultRouter());
//         let session = await router.createSession("username", "password");
//         const [timeout, category, adapter] = await Promise.all([
//             router.getSessionTimeout(),
//             router.getCategoryForClient(),
//             communicator.createObjectAdapterWithRouter("", router)]
//         )
//         console.log(timeout);
//         console.log(category);
//         console.dir(adapter);
//         const connection = router.ice_getCachedConnection();
//         // 心跳
//         // connection.setACM(undefined, undefined, Ice.ACMHeartbeat.HeartbeatAlways);
//         connection.setCloseCallback(() => {
//             //當與Glacier2連線中斷時觸發
//             console.log("Connection lost");
//         });

//         let proxy = communicator.stringToProxy("Server:tcp -h 127.0.0.1 -p 10000");
//         const serverEvent = TreeDiagram.ServerEventPrx.uncheckedCast(adapter.add(new ServerEventI(), new Ice.Identity("serverEvent", category)));
//         const server = await TreeDiagram.ServerPrx.checkedCast(proxy);
//         if (server) {
//             await server.initEvent(serverEvent);
//             // let type = document.getElementById("data").value
//             // await server.initEvent(new TreeDiagram.ServerEventPrx);
//             // await server.createTree(new TreeDiagram.Tree(uuidv1(), ""));
//             // let readTree = await server.readTree();
//             // console.dir(readTree);
//             // await server.deleteTree(readTree[0].uuid);
//         }
//         else {
//             console.log("Invalid proxy");
//         }
//     }
//     catch (ex) {
//         console.error(ex);
//         //     process.exitCode = 1;
//     }
//     finally {
//         // if (communicator) {
//         //     await communicator.destroy();
//         // }
//     }
// };
create();