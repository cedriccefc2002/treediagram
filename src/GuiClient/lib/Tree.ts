import { v4 as uuidv4 } from "uuid";
import { TreeDiagram as TreeDiagram_Tree } from "../Ice/Tree";
type TreeType = TreeDiagram_Tree.TreeType;
export class Tree extends TreeDiagram_Tree.Tree {
    public constructor(type: TreeType) {
        super();
        this.uuid = uuidv4();
        this.type = type;
    }
}