#include <TreeView.ice>

module TreeDiagram
{
    // enum ServerStatus { 
    //     Normal,
    //     Fault  
    // }

    interface ServerEvent {
        // 2-1 樹狀圖清單更新 發布
        void TreeListUpdate(); 
        // 2-2 樹狀圖資料更新 發布
        void TreeUpdate(string uuid);
        // 2-3 節點資料更新 發布
        void NodeUpdate(string uuid, string data); 
    };

    interface Server
    {
        // 查詢伺服器狀態
        // ServerStatus status();
        
        // 管理樹狀圖
        // 1-1-1 列出所有樹  
        Trees listAllTrees();
        // 1-1-2 新增樹
        idempotent void createTree(Tree tree); 
        // 1-1-3 取得樹的資料 
        Tree getTreeByUUID(string uuid);
        // 1-1-4 刪除樹
        idempotent void deleteTree(string uuid);
        // 管理節點
        long getChildrenCount(string uuid);
        // 1-2-1 新增節點
        idempotent void createNode(string rootUUID, string parentUUID, string data);
        Nodes getChildrenNode(string uuid);
        // 1-2-2 更新節點資料
        idempotent void updateNodeData(string uuid, string data);
        // 1-2-3 刪除節點與其子樹
        idempotent void deleteNodeTree(string uuid);
        // 1-2-4 移動節點與子樹
        idempotent void moveNode(string uuid, string newParent);
        // 1-2-5 刪除節點保留子樹
        idempotent void deleteNode(string uuid);
        // 1-2-6 取得所有節點資料
        TreeView getNodeView(string uuid);
        // 傳送client EventHandler
        void initEvent(ServerEvent* event);
    }
}