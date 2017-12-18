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

        public async Task<IEnumerable<JobApplication>> ReadAllAsync()
        {
            List<JobApplication> jobApplications = await _repository
                                                       .FindAll()
                                                       .Select(application => new JobApplication
                                                       {
                                                           CompanyName = application.CompanyName,
                                                           ApplicationDate = application.ApplicationDate.Date,
                                                           Status = application.CurrentStatus
                                                       })
                                                       .ToListAsync();

            return jobApplications;
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
    }
}