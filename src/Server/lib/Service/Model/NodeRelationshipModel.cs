using AutoMapper;
using Domain = Server.lib.Repository.Domain;
namespace Server.lib.Service.Model
{
    public class NodeRelationshipModel : Domain.NodeRelationshipDomain
    {
        public Domain.NodeRelationshipDomain NodeRelationshipDomain()
        {
            // return new Domain.NodeRelationshipDomain
            // {
            //     parentUUID = parentUUID,
            //     childUUID = childUUID,
            // };
            return Mapper.Map<Domain.NodeRelationshipDomain>(this);
        }
        public static NodeRelationshipModel FromDomain(Domain.NodeRelationshipDomain source)
        {
            // return new NodeRelationshipModel
            // {
            //     parentUUID = source.parentUUID,
            //     childUUID = source.childUUID,
            // };
            return Mapper.Map<NodeRelationshipModel>(source);
        }
    }
}