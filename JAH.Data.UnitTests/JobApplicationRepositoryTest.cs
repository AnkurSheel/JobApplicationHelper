using System;
using System.Linq;
using System.Threading.Tasks;
using JAH.Data.Entities;
using JAH.Data.Repositories;
using JAH.DomainModels;
using Xunit;

namespace JAH.Data.UnitTests
{
    public class JobApplicationRepositoryTest : IClassFixture<ContextFixture>
    {
        private readonly JobApplicationRepository _jobApplicationRepository;
        private readonly JobApplicationDbContext _jobApplicationDbContext;

        public JobApplicationRepositoryTest(ContextFixture fixture)
        {
            _jobApplicationDbContext = fixture.Context;
            _jobApplicationRepository = new JobApplicationRepository(_jobApplicationDbContext);
        }

        [Fact]
        public async Task ShouldReturnAllJobApplications()
        {
            var jobApplications = new[]
            {
                new JobApplicationEntity {CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), CurrentStatus = Status.None},
                new JobApplicationEntity {CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), CurrentStatus = Status.Applied},
                new JobApplicationEntity {CompanyName = "Company 3", ApplicationDate = new DateTime(2017, 11, 14), CurrentStatus = Status.Interview},
                new JobApplicationEntity {CompanyName = "Company 4", ApplicationDate = new DateTime(2017, 10, 9), CurrentStatus = Status.Offer},
                new JobApplicationEntity {CompanyName = "Company 5", ApplicationDate = new DateTime(2017, 09, 18), CurrentStatus = Status.Rejected},
            };

            foreach (JobApplicationEntity jobApplication in jobApplications)
            {
                _jobApplicationDbContext.JobApplications.Add(jobApplication);
            }

            _jobApplicationDbContext.SaveChanges();

            // Act
            var result = await _jobApplicationRepository.FindAll();

            // Assert
            Assert.Equal(jobApplications, result.ToArray());
        }
        }
    }
}
