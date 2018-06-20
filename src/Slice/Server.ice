#include <TreeView.ice>

module TreeDiagram
{
    // enum ServerStatus { 
    //     Normal,
    //     Fault  
    // }

    interface ServerEvent {
        void TreeListUpdate(); 
        void TreeUpdate(string uuid);
        void NodeUpdate(string uuid, string data); 
    };

    interface Server
    {
        // 查詢伺服器狀態
        // ServerStatus status();
        
        // 管理樹狀圖
        idempotent void createTree(Tree tree);    
        Trees listAllTrees();
        Tree getTreeByUUID(string uuid);
        idempotent void deleteTree(string uuid);
        // 管理節點
        long getChildrenCount(string uuid);
        idempotent void createNode(string rootUUID, string parentUUID, string data);
        Nodes getChildrenNode(string uuid);
        idempotent void updateNodeData(string uuid, string data);
        idempotent void deleteNodeTree(string uuid);
        idempotent void moveNode(string uuid, string newParent);
        idempotent void deleteNode(string uuid);
        TreeView getNodeView(string uuid);
        // 傳送client EventHandler
        void initEvent(ServerEvent* event);
    }
}