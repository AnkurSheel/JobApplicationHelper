using JAH.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JAH.Data
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