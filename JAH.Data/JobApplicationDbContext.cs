using JAH.Data.Entities;
using JAH.DomainModels;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobApplicationEntity>().HasKey(s => s.Id);
            modelBuilder.Entity<JobApplicationEntity>().Property(p => p.CompanyName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<JobApplicationEntity>().Property(p => p.ApplicationDate).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<JobApplicationEntity>().Property(p => p.CurrentStatus).HasDefaultValue(Status.None);
            base.OnModelCreating(modelBuilder);
        }
    }
}