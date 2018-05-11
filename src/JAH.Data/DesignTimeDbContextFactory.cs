using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JAH.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<JobApplicationDbContext>
    {
        public JobApplicationDbContext CreateDbContext(string[] args)
        {
            // IConfigurationRoot configuration = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json")
            //    .Build();
            var builder = new DbContextOptionsBuilder<JobApplicationDbContext>();
            builder.UseSqlServer("Server = (localdb)\\mssqllocaldb; Database = JobApplicationData; Trusted_Connection = True;");
            return new JobApplicationDbContext(builder.Options);
        }
    }
}
