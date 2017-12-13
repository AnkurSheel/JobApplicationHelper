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
            var jobApplications = await _repository.FindAll()
                                                   .Select(application => new JobApplication
                                                   {
                                                       Name = application.CompanyName,
                                                       StartDate = application.ApplicationDate.Date,
                                                       Status = application.CurrentStatus
                                                   })
                                                   .ToListAsync();

            return jobApplications;
        }

        public async Task Add(JobApplication jobApplication)
        {
            var jobApplicationEntity = new JobApplicationEntity()
            {
                CompanyName = jobApplication.Name,
                ApplicationDate = jobApplication.StartDate,
                CurrentStatus = jobApplication.Status
            };
            await _repository.Add(jobApplicationEntity);
        }
    }
}
