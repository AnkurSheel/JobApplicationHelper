using AutoMapper;
using JAH.Data.Entities;
using JAH.DomainModels;

namespace JAH.Api.Mappings
{
    class JobApplicationMappingProfile : Profile
    {
        public JobApplicationMappingProfile()
        {
            CreateMap<JobApplicationEntity, JobApplication>()
                .ForMember(a => a.Status, opt => opt.MapFrom(e => e.CurrentStatus))
                .ForMember(a => a.Url, opt => opt.ResolveUsing<JobApplicationUrlResolver>())
                .ReverseMap();
        }
    }
}
