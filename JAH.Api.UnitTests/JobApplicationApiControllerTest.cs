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
        public async Task ReadAllAsync_MultipleApplications_OkObjectResultWithAListOfJobApplications()
        {
            // Arrange
            var expectedjobApplications = new EnumerableQuery<JobApplication>(new[]
            {
                new JobApplication { Name = "Company 1", StartDate = new DateTime(2017, 11, 13), Status = Status.None },
                new JobApplication { Name = "Company 2", StartDate = new DateTime(2017, 11, 14), Status = Status.Applied },
                new JobApplication { Name = "Company 3", StartDate = new DateTime(2017, 11, 14), Status = Status.Offer }
            });

            _jobApplicationService.ReadAllAsync().Returns(expectedjobApplications);

            // Act
            IActionResult result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult) result;
            Assert.Equal(expectedjobApplications, okResult.Value);
        }

        [Fact]
        public async Task ReadAllAsync_NoApplications_NoContentObjectResult()
        {
            // Arrange
            var expectedjobApplications = new List<JobApplication>().AsQueryable();

            _jobApplicationService.ReadAllAsync().Returns(expectedjobApplications);

            // Act
            IActionResult result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostAsync_ApplicationDoesNotExist_CreatedResultWithJobApplication()
        {
            // Arrange
            var jobApplication = new JobApplication { Name = "Company 1", StartDate = new DateTime(2017, 11, 13), Status = Status.None };

            // Act
            IActionResult result = await _jobApplicationController.PostAsync(jobApplication);

            // Assert
            await _jobApplicationService.Received().AddNewApplication(jobApplication);

            Assert.IsType<CreatedAtActionResult>(result);
            var createdResult = (CreatedAtActionResult) result;
            Assert.Equal(jobApplication, createdResult.Value);
        }

        [Fact]
        public async Task PostAsync_ApplicationExists_BadRequestResult()
        {
            // Arrange
            var jobApplication = new JobApplication { Name = "Company 1", StartDate = new DateTime(2017, 11, 13), Status = Status.None };

            _jobApplicationService.AddNewApplication(jobApplication).Returns(x => throw new ArgumentException());

            // Act
            IActionResult result = await _jobApplicationController.PostAsync(jobApplication);

            // Assert
            await _jobApplicationService.Received().AddNewApplication(jobApplication);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
