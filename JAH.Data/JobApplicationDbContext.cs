using JobApplicationHelper.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationHelper.Data
{
    public class JobApplicationDbContext : DbContext
    {
        public JobApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<JobApplicationEntity> JobApplications { get; set; }
    }
}