using System.Collections.Generic;
using System.Threading.Tasks;
using Server.lib.Repository.Domain;

namespace Server.lib.Repository
{
    public interface IRepository
    {
        Task<bool> CreateNode(string root, string parent, string data, string isBinaryleft);
        Task<NodeDomain> GetNodeByUUID(string uuid);
        // 編輯節點資料
        Task<bool> UpdateNodeData(string uuid, string data);
        Task<bool> DeleteNodeTree(string uuid);
        Task<uint> GetChildrenCount(string uuid);
        // 刪除節點：子樹會保留
        Task<bool> DeleteNode(string uuid);
        // 搬移節點
        Task<bool> MoveNode(string uuid, string newParent, string isBinaryleft);
        // 檢視圖
        Task<TreeViewDomain> GetNodeView(string root);
        Task<List<NodeDomain>> GetChildrenNode(string parent);
        Task<bool> DeleteTree(string uuid);
        Task<bool> CreateTree(TreeDomain tree);
        Task<TreeDomain> GetTreeByUUID(string uuid);
        Task<IList<TreeDomain>> ListAllTrees();
        Task<bool> UpdateTreeType(string uuid, string type);
        Task<bool> UpdateTreeDateTime(string uuid);
        Task<bool> Status();
    }
}