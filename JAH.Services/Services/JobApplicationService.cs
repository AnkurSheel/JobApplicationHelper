using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.DomainModels;
using JAH.Services.Interfaces;

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
            var jobApplicationEntities = _repository.FindAll();
            return await Task.Run(() =>
                                  {
                                      return jobApplicationEntities.Result.Select(application => new JobApplication
                                      {
                                          Name = application.CompanyName,
                                          StartDate = application.ApplicationDate.Date,
                                          Status = application.CurrentStatus
                                      });
                                  });
        }
    }
}
