using AutoMapper;
using Model = Server.lib.Service.Model;
using Domain = Server.lib.Repository.Domain;
namespace Server.lib.Service.AutoMapper
{
    public class NodeRelationshipProfile : Profile
    {
        public NodeRelationshipProfile()
        {
            CreateMap<Model.NodeModel, Domain.NodeDomain>();
            CreateMap<Domain.NodeDomain, Model.NodeModel>();
        }
    }
}