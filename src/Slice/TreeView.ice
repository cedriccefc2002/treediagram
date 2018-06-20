#include <Tree.ice>
#include <Node.ice>
#include <NodeRelationship.ice>

module TreeDiagram
{
    class TreeView
    {
        string uuid;
        Nodes nodes;
        NodeRelationships rels;
    }

    sequence<TreeView> TreeViews;
}