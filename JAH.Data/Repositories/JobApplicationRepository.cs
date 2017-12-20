using System;
using System.Collections.Generic;
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
            _context.JobApplications.Add(jobApplication);
            await SaveAsync();
        }

        public IQueryable<JobApplicationEntity> GetAll(Expression<Func<JobApplicationEntity, bool>> filter = null)
        {
            IQueryable<JobApplicationEntity> query = _context.JobApplications;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query;
        }

        public JobApplicationEntity GetOne(Expression<Func<JobApplicationEntity, bool>> filter = null)
        {
            return GetAll(filter).SingleOrDefault();
        }

        public async Task Update(JobApplicationEntity jobApplication)
        {
            _context.JobApplications.Attach(jobApplication);
            _context.Entry(jobApplication).State = EntityState.Modified;
            await SaveAsync();
        }

        private Task SaveAsync()
        {
            try
            {
                return _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                ThrowEnhancedValidationException(e);
            }

            return Task.FromResult(0);
        }

        private void ThrowEnhancedValidationException(DbUpdateException dbu)
        {
            IEnumerable<char> errorMessages = dbu.Entries.SelectMany(x => x.Entity.GetType().Name);

            string fullErrorMessage = string.Join("; ", errorMessages);
            string exceptionMessage = string.Concat(dbu.Message, " The validation errors are: ", fullErrorMessage);
            throw new DbUpdateException(exceptionMessage, dbu);
        }
    }
}
