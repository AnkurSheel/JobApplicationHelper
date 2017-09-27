using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.Data.Interfaces;
using JobApplicationHelper.DomainModels;
using JobApplicationHelper.Services.Interfaces;
using System;

namespace JobApplicationHelper.Services.Services
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