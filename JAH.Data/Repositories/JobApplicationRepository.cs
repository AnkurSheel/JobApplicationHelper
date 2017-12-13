using System;
using System.Linq;
using System.Threading.Tasks;
using JAH.Data.Entities;
using JAH.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JAH.Data.Repositories
{
    public class JobApplicationRepository : IRepository<JobApplicationEntity>
    {
        private readonly JobApplicationDbContext _context;

        public JobApplicationRepository(JobApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Add(JobApplicationEntity jobApplication)
        {
            bool existingApplication = await _context.JobApplications.AnyAsync(a => a.CompanyName == jobApplication.CompanyName &&
                                                                                   a.ApplicationDate == jobApplication.ApplicationDate);
            if (!existingApplication)
            {
                _context.JobApplications.Add(jobApplication);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Trying to add same application", $"{jobApplication.CompanyName} {jobApplication.ApplicationDate}");
            }
        }

        public IQueryable<JobApplicationEntity> FindAll()
        {
            return _context.JobApplications;
        }
    }
}
