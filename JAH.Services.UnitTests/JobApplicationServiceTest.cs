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
                                                                 new JobApplication { Name = "Company 1" },
                                                                 new JobApplication { Name = "Company 2" },
                                                                 new JobApplication { Name = "Company 3" }
                                                             });
            _jobApplicationRepository.FindAll().Returns(expectedJobApplications);

            // Act
            var result = await _jobApplicationService.ReadAllAsync();
            
            // Assert
            Assert.Equal(expectedJobApplications, result);
        }
    }
}
