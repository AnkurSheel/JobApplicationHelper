using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAH.Data.Interfaces;
using JAH.DomainModels;
using JAH.Services.Services;
using NSubstitute;
using Xunit;

namespace JAH.Services.UnitTests
{
    public class JobApplicationServiceTest
    {
        private readonly JobApplicationService _jobApplicationService;
        private readonly IRepository<JobApplication> _jobApplicationRepository;

        public JobApplicationServiceTest()
        {
            _jobApplicationRepository = Substitute.For<IRepository<JobApplication>>();
            _jobApplicationService = new JobApplicationService(_jobApplicationRepository);
        }

        [Fact]
        public async Task ShouldReturnAllJobApplications()
        {
            var expectedJobApplications = new EnumerableQuery<JobApplication>(new List<JobApplication>
            {
                new JobApplication {Name = "Company 1", StartDate = new DateTime(2017, 11, 13)},
                new JobApplication {Name = "Company 2", StartDate = new DateTime(2017, 11, 14)},
                new JobApplication {Name = "Company 3", StartDate = new DateTime(2017, 11, 14)}
            });
            _jobApplicationRepository.FindAll().Returns(expectedJobApplications);

            // Act
            var result = await _jobApplicationService.ReadAllAsync();

            // Assert
            Assert.Equal(expectedJobApplications, result);
        }
    }
}
