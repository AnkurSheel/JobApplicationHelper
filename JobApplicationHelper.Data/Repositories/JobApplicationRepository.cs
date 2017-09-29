using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.Data.Interfaces;
using JobApplicationHelper.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Data.Repositories
{
    public class JobApplicationRepository : IRepository<JobApplication>
    {
        private readonly DbContext _context;

        public JobApplicationRepository(DbContext context)
        {
            _context = context;
        }

        public Task<IQueryable<JobApplication>> FindAll()
        {
            throw new System.NotImplementedException();
        }
    }
}