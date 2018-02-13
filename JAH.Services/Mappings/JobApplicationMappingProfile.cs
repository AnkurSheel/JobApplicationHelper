using AutoMapper;
using JAH.Data.Entities;
using JAH.DomainModels;

namespace JAH.Services.Mappings
{
    class JobApplicationMappingProfile : Profile
    {
        public JobApplicationMappingProfile()
        {
            CreateMap<JobApplicationEntity, JobApplication>().ReverseMap();
        }
    }
}
