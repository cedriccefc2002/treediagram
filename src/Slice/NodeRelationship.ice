module TreeDiagram
{
    class NodeRelationship
    {
        string parentUUID;
        string childUUID;
    }

    sequence<NodeRelationship> NodeRelationships;
}