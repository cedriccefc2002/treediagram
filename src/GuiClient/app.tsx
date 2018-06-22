import * as React from "react";
import * as ReactDOM from "react-dom";

import { configure } from "log4js";

import { Proxy } from "./lib/IceProxy/IceProxy";

import { Render } from "./lib/Render";

if (require.main === module) {
    (async () => {
        try {
            configure({
                appenders: { console: { type: "console" } },
                categories: { default: { appenders: ["console"], level: "all" } },
            });
            ReactDOM.render(<Render compiler="TypeScript" framework="React" />, document.getElementById("content"));
            await import("jquery");
            await import("bootstrap");
            const proxy = await Proxy.GetProxy();
            await proxy.init();
        } catch (error) {
            alert(error);
            window.close();
        }
    })();
}
