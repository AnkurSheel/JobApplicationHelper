using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public JobApplicationService(IRepository<JobApplicationEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<JobApplication>> GetAllApplications()
        {
            List<JobApplication> jobApplications = await _repository
                                                       .GetAll()
                                                       .Select(application => new JobApplication
                                                       {
                                                           Id = application.Id,
                                                           CompanyName = application.CompanyName,
                                                           ApplicationDate = application.ApplicationDate.Date,
                                                           Status = application.CurrentStatus
                                                       })
                                                       .ToListAsync();

            return jobApplications;
        }

        public async Task<JobApplication> GetApplication(string companyName)
        {
            JobApplicationEntity jobApplication = _repository.GetOne(f => f.CompanyName.Equals(companyName));
            if (jobApplication != null)
            {
                return await Task.Run(() => new JobApplication
                {
                    Id = jobApplication.Id,
                    ApplicationDate = jobApplication.ApplicationDate,
                    CompanyName = jobApplication.CompanyName,
                    Status = jobApplication.CurrentStatus
                });
            }

            return null;
        }

        public async Task<JobApplication> AddNewApplication(JobApplication jobApplication)
        {
            var jobApplicationEntity = new JobApplicationEntity
            {
                Id = jobApplication.Id,
                CompanyName = jobApplication.CompanyName,
                ApplicationDate = jobApplication.ApplicationDate,
                CurrentStatus = jobApplication.Status
            };
            await _repository.Create(jobApplicationEntity);
            return new JobApplication
            {
                Id = jobApplicationEntity.Id,
                CompanyName = jobApplicationEntity.CompanyName,
                ApplicationDate = jobApplicationEntity.ApplicationDate,
                Status = jobApplicationEntity.CurrentStatus
            };
        }

        public async Task UpdateApplication(JobApplication jobApplication)
        {
            var jobApplicationEntity = new JobApplicationEntity
            {
                Id = jobApplication.Id,
                CompanyName = jobApplication.CompanyName,
                ApplicationDate = jobApplication.ApplicationDate,
                CurrentStatus = jobApplication.Status
            };
            await _repository.Update(jobApplicationEntity);
        }
    }
}
