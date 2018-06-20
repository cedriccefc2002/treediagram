using System.Collections.Generic;
using System.Linq;
using Domain = Server.lib.Repository.Domain;
namespace Server.lib.Service.Model
{
    public class TreeViewModel
    {
        public string uuid { get; set; }
        public List<NodeModel> nodes { get; set; }
        public List<NodeRelationshipModel> rels { get; set; }
        public static TreeViewModel FromDomain(Domain.TreeViewDomain source)
        {
            return new TreeViewModel
            {
                uuid = source.uuid,
                nodes = source.nodes.Select(a => Model.NodeModel.FromDomain(a)).ToList(),
                rels = source.rels.Select(a => (NodeRelationshipModel)a).ToList(),
            };
        }
    }
}