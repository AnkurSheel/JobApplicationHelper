using JAH.Data.Entities;
using JAH.DomainModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JAH.Data
{
    public class JobApplicationDbContext : IdentityDbContext<JobApplicationUser>
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
            modelBuilder.Entity<JobApplicationEntity>().Property(p => p.CurrentStatus).HasDefaultValue(Status.Applied);

            modelBuilder.Entity<JobApplicationEntity>()
                        .HasOne(application => application.Owner)
                        .WithMany(user => user.Applications)
                        .IsRequired()
                        .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
