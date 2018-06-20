#include <Tree.ice>

module TreeDiagram
{
    sequence<Tree> Trees;

    enum ServerStatus { 
        Normal,
        Fault  
    }

    interface ServerEvent {
        void TreeListUpdate(); 
        void TreeUpdate(string uuid);
        void NodeUpdate(string uuid, string data); 
    };

    interface Server
    {
        // 查詢伺服器狀態
        ServerStatus status();
        
        // 管理樹狀圖
        idempotent void createTree(Tree tree);    
        Trees readTree();
        Tree readSingleTree(string uuid);
        idempotent void updateTree(Tree tree);
        idempotent void deleteTree(string uuid);
        void initEvent(ServerEvent* event);
    }
}