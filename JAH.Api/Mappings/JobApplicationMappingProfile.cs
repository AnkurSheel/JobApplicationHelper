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
                .ForMember(c => c.Url, opt => opt.ResolveUsing<JobApplicationUrlResolver>())
                .ReverseMap();
        }
    }
}
