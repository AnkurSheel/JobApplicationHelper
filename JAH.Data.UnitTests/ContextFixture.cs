using System;
using Microsoft.EntityFrameworkCore;

namespace JAH.Data.UnitTests
{
    public static class ContextFixture
    {
        public static JobApplicationDbContext GetContextWithData()
        {
            var options = new DbContextOptionsBuilder<JobApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new JobApplicationDbContext(options);

            context.SaveChanges();

            return context;
        }
    }
}
