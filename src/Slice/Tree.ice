// #include <Node.ice>

module TreeDiagram
{
    // sequence<Node> Nodes;

    enum TreeType { 
        Normal,
        Binary  
    }

    // class TreeNode extends Node
    // {
    //     Nodes children;
    // }

    class Tree // extends Node
    {
        string uuid;
        TreeType type;
    }

    sequence<Tree> Trees;
}