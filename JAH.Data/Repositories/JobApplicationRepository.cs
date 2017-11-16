using JAH.Data.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using JAH.DomainModels;

namespace JAH.Data.Repositories
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
            var jobApplications = _context.JobApplications.Select(application => new JobApplication()
            {
                Name = application.CompanyName,
                StartDate = application.ApplicationDate.Date,
                Status = application.CurrentStatus
            });
            return await Task.Run(() => jobApplications);
        }
    }
}