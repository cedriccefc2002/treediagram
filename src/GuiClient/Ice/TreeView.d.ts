import { Ice, Glacier2 } from "ice";

import { TreeDiagram as TreeDiagram_Node } from "./Node";
import { TreeDiagram as TreeDiagram_NodeRelationship } from "./NodeRelationship";

export namespace TreeDiagram {
    export class TreeView extends Ice.Object {
        uuid: string;
        nodes: TreeDiagram_Node.Node[];
        rels: TreeDiagram_NodeRelationship.NodeRelationship[];
    }
    export class TreeViewPrx extends Ice.ObjectPrx { }
}