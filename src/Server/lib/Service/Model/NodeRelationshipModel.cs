using Domain = Server.lib.Repository.Domain;
namespace Server.lib.Service.Model
{
    public class NodeRelationshipModel : Domain.NodeRelationshipDomain
    {
        public Domain.NodeRelationshipDomain NodeRelationshipDomain()
        {
            return new Domain.NodeRelationshipDomain
            {
                parentUUID = parentUUID,
                childUUID = childUUID,
            };
        }
         public static NodeRelationshipModel FromDomain(Domain.NodeRelationshipDomain source)
        {
            return new NodeRelationshipModel
            {
                parentUUID = source.parentUUID,
                childUUID = source.childUUID,
            };
        }
    }
}