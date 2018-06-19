using System.Collections.Generic;

namespace Server.lib.Repository.Domain
{
    public class TreeViewDomain
    {
        public string uuid { get; set; }
        public IList<NodeDomain> nodes { get; set; }
        public IList<NodeRelationshipDomain> rels { get; set; }
    }
}