using System;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task Create(JobApplicationEntity jobApplication)
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

        public IQueryable<JobApplicationEntity> GetAll(Expression<Func<JobApplicationEntity, bool>> filter = null)
        {
            IQueryable<JobApplicationEntity> query = _context.Set<JobApplicationEntity>();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query;
        }

        public JobApplicationEntity GetOne(Expression<Func<JobApplicationEntity, bool>> filter = null)
        {
            return null;
            //return GetAll(filter).SingleOrDefault();
        }
    }
}
