using Domain = Server.lib.Repository.Domain;
namespace Server.lib.Service.Model
{
    public enum TreeType
    {
        Normal,
        Binary
    }
    public class TreeModel
    {
        public string uuid { get; set; }
        public TreeType type { get; set; }
        public Domain.TreeDomain TreeDomain()
        {
            return new Domain.TreeDomain
            {
                uuid = uuid,
                type = type == TreeType.Binary ? "Binary" : "Normal"
            };
        }
        public static TreeModel FromDomain(Domain.TreeDomain source)
        {
            return new TreeModel
            {
                uuid = source.uuid,
                type = source.type == "Binary" ? TreeType.Binary : TreeType.Normal
            };
        }
    }
}