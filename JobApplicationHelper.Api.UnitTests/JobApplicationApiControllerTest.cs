using System.Linq;
using System.Threading.Tasks;
using JobApplicationHelper.Api.Controllers;
using JobApplicationHelper.DomainModels;
using JobApplicationHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace JobApplicationHelper.Api.UnitTests
{
    public class JobApplicationApiControllerTest
    {
        private readonly JobApplicationApiController _jobApplicationController;
        private readonly IJobApplicationService _jobApplicationService;

        public JobApplicationApiControllerTest()
        {
            _jobApplicationService = Substitute.For<IJobApplicationService>();
            _jobApplicationController = new JobApplicationApiController(_jobApplicationService);
        }

        [Fact]
        public async Task ShouldReturnOkObjectResultWithAListOfJobApplications()
        {
            // Arrange
            var expectedjobApplications = new EnumerableQuery<JobApplication>(new[]
                                                                              {
                                                                                  new JobApplication { Name = "Company 1" },
                                                                                  new JobApplication { Name = "Company 2" },
                                                                                  new JobApplication { Name = "Company 3" }
                                                                              });

            _jobApplicationService.ReadAllAsync().Returns(expectedjobApplications);

            // Act
            var result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(expectedjobApplications, okResult.Value);
        }
    }
}
