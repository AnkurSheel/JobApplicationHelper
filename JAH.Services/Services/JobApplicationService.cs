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
                    ApplicationDate = jobApplication.ApplicationDate,
                    CompanyName = jobApplication.CompanyName,
                    Status = jobApplication.CurrentStatus
                });
            }

            return null;
        }

        public async Task AddNewApplication(JobApplication jobApplication)
        {
            var jobApplicationEntity = new JobApplicationEntity
            {
                CompanyName = jobApplication.CompanyName,
                ApplicationDate = jobApplication.ApplicationDate,
                CurrentStatus = jobApplication.Status
            };
            await _repository.Create(jobApplicationEntity);
        }

        public async Task UpdateApplication(JobApplication jobApplication)
        {
            var jobApplicationEntity = new JobApplicationEntity
            {
                CompanyName = jobApplication.CompanyName,
                ApplicationDate = jobApplication.ApplicationDate,
                CurrentStatus = jobApplication.Status
            };
            await _repository.Update(jobApplicationEntity);
        }
    }
}
