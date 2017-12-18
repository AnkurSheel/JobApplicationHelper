using System;
using System.Linq;
using JAH.Data.Entities;
using JAH.DomainModels;

namespace JAH.Data
{
    public static class DbInitializer
    {
        public static void Initialize(JobApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.JobApplications.Any())
            {
                // database has been seeded
                return;
            }

            var jobApplications = new[]
            {
                new JobApplicationEntity { CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), CurrentStatus = Status.Rejected },
                new JobApplicationEntity { CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), CurrentStatus = Status.Applied },
                new JobApplicationEntity
                {
                    CompanyName = "Company 3",
                    ApplicationDate = new DateTime(2017, 11, 14),
                    CurrentStatus = Status.Interview
                },
                new JobApplicationEntity { CompanyName = "Company 4", ApplicationDate = new DateTime(2017, 10, 9), CurrentStatus = Status.Offer }
            };

            foreach (JobApplicationEntity jobApplication in jobApplications)
            {
                context.JobApplications.Add(jobApplication);
            }

            context.SaveChanges();
        }
    }
}
