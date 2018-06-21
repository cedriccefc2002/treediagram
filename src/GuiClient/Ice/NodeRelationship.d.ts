import { Ice, Glacier2 } from "ice";

export namespace TreeDiagram {
    export class NodeRelationship extends Ice.Object {
        parentUUID: string;
        childUUID: string;
    }
    export class NodeRelationshipPrx extends Ice.ObjectPrx { }
}