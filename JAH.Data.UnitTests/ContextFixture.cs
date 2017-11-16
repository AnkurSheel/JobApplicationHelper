using System;
using JAH.Data.Entities;
using JAH.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace JAH.Data.UnitTests
{
    public class ContextFixture : IDisposable
    {
        public ContextFixture()
        {
            Context = GetContextWithData();
        }

        public JobApplicationDbContext Context { get; }

        private JobApplicationDbContext GetContextWithData()
        {
            var options = new DbContextOptionsBuilder<JobApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new JobApplicationDbContext(options);

            context.JobApplications.Add(new JobApplicationEntity
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                CurrentStatus = Status.None
            });
            context.JobApplications.Add(new JobApplicationEntity
            {
                CompanyName = "Company 2",
                ApplicationDate = new DateTime(2017, 11, 14),
                CurrentStatus = Status.Applied
            });
            context.JobApplications.Add(new JobApplicationEntity
            {
                CompanyName = "Company 3",
                ApplicationDate = new DateTime(2017, 11, 14),
                CurrentStatus = Status.Offer
            });
            context.SaveChanges();

            return context;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Context.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
