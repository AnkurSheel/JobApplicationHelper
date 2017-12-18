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

        {
            // Arrange
            var expectedjobApplications = new EnumerableQuery<JobApplication>(new[]
            {
                new JobApplication { CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), Status = Status.Interview },
                new JobApplication { CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Applied },
                new JobApplication { CompanyName = "Company 3", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Offer }
            });

            _jobApplicationService.ReadAllAsync().Returns(expectedjobApplications);
        [Fact]
        public async Task ListAllApplications_MultipleApplications_OkObjectResultWithAListOfJobApplications()

            // Act
            IActionResult result = await _jobApplicationController.ListAllApplications();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult) result;
            Assert.Equal(expectedjobApplications, okResult.Value);
        }

        [Fact]
        public async Task ListAllApplications_NoApplications_NoContentObjectResult()
        {
            // Arrange
            IQueryable<JobApplication> expectedjobApplications = new List<JobApplication>().AsQueryable();

            _jobApplicationService.ReadAllAsync().Returns(expectedjobApplications);

            // Act
            IActionResult result = await _jobApplicationController.ListAllApplications();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void AddNewApplication_ApplicationDoesNotExist_CreatedResultWithJobApplication()
        {
            // Arrange
            var jobApplication = new JobApplication
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                Status = Status.Interview
            };

            // Act
            IActionResult result = await _jobApplicationController.AddNewApplication(jobApplication);

            // Assert
            await _jobApplicationService.Received().AddNewApplication(jobApplication);

            Assert.IsType<CreatedAtActionResult>(result);
            var createdResult = (CreatedAtActionResult) result;
            Assert.Equal(jobApplication, createdResult.Value);
        }

        [Fact]
        public async Task AddNewApplication_ApplicationExists_BadRequestResult()
        {
            // Arrange
            var jobApplication = new JobApplication
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                Status = Status.Interview
            };

            _jobApplicationService.AddNewApplication(jobApplication).Returns(x => throw new ArgumentException());

            // Act
            IActionResult result = await _jobApplicationController.AddNewApplication(jobApplication);

            // Assert
            await _jobApplicationService.Received().AddNewApplication(jobApplication);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
