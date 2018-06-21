import { Ice, Glacier2 } from "ice";

export namespace TreeDiagram {
    export enum TreeType {
        Normal,
        Binary
    }
    export class Tree extends Ice.Object {
        uuid: string;
        type: TreeType;
    }
    export class TreePrx extends Ice.ObjectPrx { }
}