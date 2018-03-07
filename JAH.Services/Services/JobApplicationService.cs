using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.DomainModels;
using JAH.Services.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace JAH.Services.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IRepository<JobApplicationEntity> _repository;
        private readonly IMapper _mapper;
        private readonly IUserResolverService _userResolver;

        public JobApplicationService(IRepository<JobApplicationEntity> repository, IMapper mapper, IUserResolverService userResolver)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper;
            _userResolver = userResolver;
        }

        public async Task<IEnumerable<JobApplication>> GetAllApplications()
        {
            var user = await _userResolver.GetCurrentUser();
            List<JobApplicationEntity> jobApplications = await _repository.GetAll(f => f.Owner.Equals(user)).ToListAsync();

            return _mapper.Map<IEnumerable<JobApplication>>(jobApplications);
        }

        public JobApplication GetApplication(string companyName)
        {
            var user = _userResolver.GetCurrentUser().Result;
            JobApplicationEntity jobApplication = _repository.GetOne(f => f.Owner.Equals(user) && f.CompanyName.Equals(companyName));
            if (jobApplication != null)
            {
                return _mapper.Map<JobApplication>(jobApplication);
            }

            return null;
        }

        public async Task<JobApplication> AddNewApplication(JobApplication jobApplication)
        {
            var user = await _userResolver.GetCurrentUser();
            JobApplicationEntity jobApplicationEntity =
                _mapper.Map<JobApplication, JobApplicationEntity>(jobApplication, opt => opt.AfterMap((src, dest) => dest.Owner = user));
            await _repository.Create(jobApplicationEntity);
            return _mapper.Map<JobApplication>(jobApplicationEntity);
        }

        public async Task<JobApplication> UpdateApplication(string companyName, JobApplication newApplication)
        {
            var user = await _userResolver.GetCurrentUser();
            JobApplicationEntity oldJobApplication = _repository.GetOne(f => f.Owner.Equals(user) && f.CompanyName.Equals(companyName));
            if (oldJobApplication == null)
            {
                return null;
            }

            _mapper.Map(newApplication, oldJobApplication);

            await _repository.Update(oldJobApplication);
            return _mapper.Map<JobApplication>(oldJobApplication);
        }
    }
}
