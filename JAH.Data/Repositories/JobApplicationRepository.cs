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
            bool existingApplication = await _context.JobApplications.AnyAsync(a => a.CompanyName == jobApplication.CompanyName);
            if (!existingApplication)
            {
                _context.JobApplications.Add(jobApplication);
                await SaveAsync();
            }
            else
            {
                throw new ArgumentException("Trying to add same application", $"{jobApplication.CompanyName} {jobApplication.ApplicationDate}");
            }
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

        protected virtual Task SaveAsync()
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
        protected virtual void ThrowEnhancedValidationException(DbUpdateException dbu)
        {
            var errorMessages = dbu.Entries.SelectMany(x => x.Entity.GetType().Name);

            var fullErrorMessage = string.Join("; ", errorMessages);
            var exceptionMessage = string.Concat(dbu.Message, " The validation errors are: ", fullErrorMessage);
            throw new DbUpdateException(exceptionMessage, dbu);
        }
    }
}
