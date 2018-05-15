using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace JAH.Data.UnitTests
{
    public static class ContextFixture
    {
        private static readonly LoggerFactory MyLoggerFactory =
            new LoggerFactory(new[] { new DebugLoggerProvider((_, level) => level >= LogLevel.Information) });

        public static JobApplicationDbContext GetContext(Guid id)
        {
            DbContextOptions<JobApplicationDbContext> options = new DbContextOptionsBuilder<JobApplicationDbContext>()
                .UseInMemoryDatabase(id.ToString())
                .UseLoggerFactory(MyLoggerFactory)
                .EnableSensitiveDataLogging()
                .Options;
            var context = new JobApplicationDbContext(options);

            context.SaveChanges();

            return context;
        }
    }
}
