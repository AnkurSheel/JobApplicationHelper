using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAH.Api.Controllers;
using JAH.DomainModels;
using JAH.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace JAH.Api.UnitTests
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
                new JobApplication {Name = "Company 1", StartDate = new DateTime(2017, 11, 13)},
                new JobApplication {Name = "Company 2", StartDate = new DateTime(2017, 11, 14)},
                new JobApplication {Name = "Company 3", StartDate = new DateTime(2017, 11, 14)}
            });

            _jobApplicationService.ReadAllAsync().Returns(expectedjobApplications);

            // Act
            var result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult) result;
            Assert.Equal(expectedjobApplications, okResult.Value);
        }

        [Fact]
        public async Task ShouldReturnNoContentObjectResultWhenNoJobApplicationsExist()
        {
            // Arrange
            var expectedjobApplications = new List<JobApplication>().AsQueryable();

            _jobApplicationService.ReadAllAsync().Returns(expectedjobApplications);

            // Act
            IActionResult result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
