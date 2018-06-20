module TreeDiagram
{
    // 節點可儲存資料 (string)
    class Node
    {
        string uuid;
        string data;
        string root;
        string parent;
    }

    sequence<Node> Nodes;
}