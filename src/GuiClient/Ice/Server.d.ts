import { Ice, Glacier2 } from "ice";

import { TreeDiagram as TreeDiagram_Node } from "./Node";
import { TreeDiagram as TreeDiagram_Tree } from "./Tree";
import { TreeDiagram as TreeDiagram_TreeView } from "./TreeView";

export namespace TreeDiagram {
    export class ServerEvent extends Ice.Object { }
    export class ServerEventPrx extends Ice.ObjectPrx { }
    export class ServerPrx extends Ice.ObjectPrx {
        createTree(tree: TreeDiagram_Tree.Tree): void;
        listAllTrees(): Promise<TreeDiagram_Tree.Tree[]>;
        getTreeByUUID(uuid: string): Promise<TreeDiagram_Tree.Tree>;
        deleteTree(uuid: string): Promise<void>;
        getChildrenCount(uuid: string): Promise<number>;
        createNode(rootUUID: string, parentUUID: string, data: string): Promise<void>;
        getChildrenNode(uuid: string): Promise<TreeDiagram_Node.Node[]>;
        updateNodeData(uuid: string, data: string): Promise<void>;
        deleteNodeTree(uuid: string): Promise<void>;
        moveNode(uuid: string, newParent: string): Promise<void>;
        deleteNode(uuid: string): Promise<void>;
        getNodeView(uuid: string): Promise<TreeDiagram_TreeView.TreeView>;
        initEvent(event: ServerEventPrx): Promise<void>;
    }
}