using Domain = Server.lib.Repository.Domain;
namespace Server.lib.Service.Model
{
    public class NodeModel
    {
        public string uuid { get; set; }
        public string root { get; set; }
        public string parent { get; set; }
        public string data { get; set; }
        public bool isBinaryleft { get; set; }
        public Domain.NodeDomain NodeDomain()
        {
            return new Domain.NodeDomain
            {
                uuid = uuid,
                root = root,
                parent = parent,
                data = data,
                isBinaryleft = IsBinaryleft(isBinaryleft)
            };
        }
        public static string IsBinaryleft(bool source){
            return source ? "true" : "false";
        }
        public static bool IsBinaryleft(string source){
            return (source == "true");
        }
        public static NodeModel FromDomain(Domain.NodeDomain source)
        {
            return new NodeModel
            {
                uuid = source.uuid,
                root = source.root,
                parent = source.parent,
                data = source.data,
                isBinaryleft = IsBinaryleft(source.isBinaryleft),
            };
        }
    }
}