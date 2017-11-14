using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAH.Data.Repositories;
using JAH.DomainModels;
using Xunit;

namespace JAH.Data.UnitTests
{
    public class JobApplicationRepositoryTest : IClassFixture<ContextFixture>
    {
        private readonly JobApplicationRepository _jobApplicationRepository;

        public JobApplicationRepositoryTest(ContextFixture fixture)
        {
            _jobApplicationRepository = new JobApplicationRepository(fixture.Context);
        }

        [Fact]
        public async Task ShouldReturnAllJobApplications()
        {
            var expectedJobApplications = new List<JobApplication>
            {
                new JobApplication {Name = "Company 1", StartDate = new DateTime(2017, 11, 13)},
                new JobApplication {Name = "Company 2", StartDate = new DateTime(2017, 11, 14)},
                new JobApplication {Name = "Company 3", StartDate = new DateTime(2017, 11, 14)}
            }.AsQueryable();

            // Act
            var result = await _jobApplicationRepository.FindAll();

            // Assert
            Assert.Equal(expectedJobApplications, result);
        }

       
    }
}
