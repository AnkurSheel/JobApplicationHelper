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

        public JobApplicationService(IRepository<JobApplicationEntity> repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<JobApplication>> GetAllApplications()
        {
            List<JobApplicationEntity> jobApplications = await _repository.GetAll().ToListAsync();

            return _mapper.Map<IEnumerable<JobApplication>>(jobApplications);
        }

        public JobApplication GetApplication(string companyName)
        {
            JobApplicationEntity jobApplication = _repository.GetOne(f => f.CompanyName.Equals(companyName));
            if (jobApplication != null)
            {
                return _mapper.Map<JobApplication>(jobApplication);
            }

            return null;
        }

        public async Task<JobApplication> AddNewApplication(JobApplication jobApplication)
        {
            var jobApplicationEntity = _mapper.Map<JobApplicationEntity>(jobApplication);
            await _repository.Create(jobApplicationEntity);
            return _mapper.Map<JobApplication>(jobApplicationEntity);
        }

        public async Task UpdateApplication(JobApplication oldApplication, JobApplication newApplication)
        {
            var jobApplicationEntity = new JobApplicationEntity
            {
                Id = oldApplication.Id,
                CompanyName = newApplication.CompanyName ?? oldApplication.CompanyName,
                ApplicationDate =
                    newApplication.ApplicationDate != DateTime.MinValue ? newApplication.ApplicationDate : oldApplication.ApplicationDate,
                CurrentStatus = newApplication.Status
            };
            await _repository.Update(jobApplicationEntity);
        }
    }
}
