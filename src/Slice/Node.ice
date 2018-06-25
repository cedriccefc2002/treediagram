module TreeDiagram
{
    // 節點可儲存資料 (string)
    class Node
    {
        string uuid;
        string data;
        string root;
        string parent;
        // 二元樹下是否為左子樹
        bool isBinaryleft;
    }

    sequence<Node> Nodes;
}