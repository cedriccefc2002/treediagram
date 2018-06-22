import * as React from "react";
import * as ReactDOM from "react-dom";

import {Render} from "./lib/Render";

ReactDOM.render(<Render compiler="TypeScript" framework="React" />, document.getElementById("content"));
