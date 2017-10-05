using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.Data.Interfaces;
using JobApplicationHelper.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Data.Repositories
{
    public class JobApplicationRepository : IRepository<JobApplication>
    {
        private readonly JobApplicationDbContext _context;

        public JobApplicationRepository(JobApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<JobApplication>> FindAll()
        {
            var jobApplications = _context.JobApplications.Select(application => new JobApplication() { Name = application.CompanyName });
            return await Task.Run(() =>
                                  {
                                      return jobApplications;
                                  });
        }
    }
}