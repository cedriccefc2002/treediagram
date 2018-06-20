using System.Collections.Generic;

namespace Server.lib.Repository.Domain
{
    public class TreeViewDomain
    {
        public string uuid { get; set; }
        public List<NodeDomain> nodes { get; set; }
        public List<NodeRelationshipDomain> rels { get; set; }
    }
}