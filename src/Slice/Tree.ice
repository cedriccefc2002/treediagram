#include <Node.ice>

module TreeDiagram
{
    sequence<Node> Nodes;

    enum TreeType { 
        Normal,
        Binary  
    }

    class TreeNode extends Node
    {
        Nodes children;
    }

    class Tree extends Node
    {
        TreeType type;
        TreeNode root;
    }
}