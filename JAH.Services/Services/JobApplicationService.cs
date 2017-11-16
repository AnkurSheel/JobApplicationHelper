using System.Linq;
using System.Threading.Tasks;
using System;
using JAH.Data.Interfaces;
using JAH.DomainModels;
using JAH.Services.Interfaces;

namespace JAH.Services.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IRepository<JobApplication> _repository;

        public JobApplicationService(IRepository<JobApplication> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task<IQueryable<JobApplication>> ReadAllAsync()
        {
            return _repository.FindAll();
        }
    }
}