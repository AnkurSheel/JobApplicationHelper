using System;
using Microsoft.EntityFrameworkCore;

namespace JAH.Data.UnitTests
{
    public static class ContextFixture
    {
        public static JobApplicationDbContext GetContext(Guid guid)
        {
            DbContextOptions<JobApplicationDbContext> options = new DbContextOptionsBuilder<JobApplicationDbContext>()
                .UseInMemoryDatabase(guid.ToString())
                .EnableSensitiveDataLogging()
                .Options;
            var context = new JobApplicationDbContext(options);

            context.SaveChanges();

            return context;
        }
    }
}
