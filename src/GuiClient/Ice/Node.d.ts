import { Ice, Glacier2 } from "ice";

export namespace TreeDiagram {
    export class Node extends Ice.Object {
        uuid: string;
        data: string;
        root: string;
        parent: string;
        isBinaryleft: boolean;
    }
    export class NodePrx extends Ice.ObjectPrx { }
}