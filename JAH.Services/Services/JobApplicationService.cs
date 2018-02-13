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

        public async Task<JobApplication> UpdateApplication(string companyName, JobApplication newApplication)
        {
            JobApplicationEntity oldJobApplication = _repository.GetOne(f => f.CompanyName.Equals(companyName));
            if (oldJobApplication == null)
            {
                return null;
            }

            oldJobApplication.CompanyName = newApplication.CompanyName ?? oldJobApplication.CompanyName;
            oldJobApplication.ApplicationDate = newApplication.ApplicationDate != DateTime.MinValue
                                                    ? newApplication.ApplicationDate
                                                    : oldJobApplication.ApplicationDate;
            oldJobApplication.CurrentStatus = newApplication.Status;
            await _repository.Update(oldJobApplication);
            return _mapper.Map<JobApplication>(oldJobApplication);
        }
    }
}
