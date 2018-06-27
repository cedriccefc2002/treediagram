using AutoMapper;
using Model = Server.lib.Service.Model;
using Domain = Server.lib.Repository.Domain;
namespace Server.lib.Service.AutoMapper
{
    public class NodeProfile : Profile
    {
        public NodeProfile()
        {
            CreateMap<Model.NodeModel, Domain.NodeDomain>();
            CreateMap<Domain.NodeDomain, Model.NodeModel>();
        }
    }
}