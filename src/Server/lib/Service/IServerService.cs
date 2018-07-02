using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.lib.Config;
using Server.lib.Repository;
using Server.lib.Service.Model;
namespace Server.lib.Service
{
    public interface IServerService
    {
        Task<bool> Status();
        Task<bool> createTree(Model.TreeModel tree);

        Task<List<Model.TreeModel>> ListAllTrees();

        Task<bool> DeleteTree(string uuid);

        Task<TreeModel> GetTreeByUUID(string uuid);
        Task<NodeModel> GetNodeByUUID(string uuid);
        Task<uint> GetChildrenCount(string uuid);
        Task<bool> CreateNode(string rootUUID, string parentUUID, string data);

        Task<List<Model.NodeModel>> GetChildrenNode(string uuid);
        Task<bool> UpdateNodeData(string uuid, string data);

        Task<bool> DeleteNodeTree(string uuid);
        Task<bool> MoveNode(string uuid, string newParent);

        Task<bool> DeleteNode(string uuid);
        Task<TreeViewModel> GetNodeView(string uuid);
    }
}