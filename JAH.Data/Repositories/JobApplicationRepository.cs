using System.Linq;
using System.Threading.Tasks;
using JAH.Data.Entities;
using JAH.Data.Interfaces;

namespace JAH.Data.Repositories
{
    public class JobApplicationRepository : IRepository<JobApplicationEntity>
    {
        private readonly JobApplicationDbContext _context;

        public JobApplicationRepository(JobApplicationDbContext context)
        {
            _context = context;
        }

        {
            var jobApplications = _context.JobApplications.Select(application => new JobApplication()
        public async Task<IQueryable<JobApplicationEntity>> FindAll()
            {
            return await Task.Run(() => _context.JobApplications);
        }
    }
}