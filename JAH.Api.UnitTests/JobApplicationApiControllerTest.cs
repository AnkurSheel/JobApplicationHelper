using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAH.Api.Controllers;
using JAH.DomainModels;
using JAH.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace JAH.Api.UnitTests
{
    public class JobApplicationApiControllerTest
    {
        private readonly JobApplicationsController _jobApplicationsController;
        private readonly IJobApplicationService _jobApplicationService;
        private readonly EnumerableQuery<JobApplication> _expectedjobApplications;

        public JobApplicationApiControllerTest()
        {
            var logger = Substitute.For<ILogger<JobApplicationsController>>();

            _jobApplicationService = Substitute.For<IJobApplicationService>();
            _jobApplicationsController = new JobApplicationsController(_jobApplicationService, logger);

            _expectedjobApplications = new EnumerableQuery<JobApplication>(new[]
            {
                new JobApplication { Id = 1, CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), Status = Status.Interview },
                new JobApplication { Id = 2, CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Applied },
                new JobApplication { Id = 3, CompanyName = "Company 3", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Offer }
            });
        }

        [Fact]
        public async Task ListAllApplications_MultipleApplications_OkObjectResultWithAListOfJobApplications()
        {
            // Arrange

            _jobApplicationService.GetAllApplications().Returns(_expectedjobApplications);

            // Act
            IActionResult result = await _jobApplicationsController.ListAllApplications();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(_expectedjobApplications, okResult.Value);
        }

        [Fact]
        public async Task ListAllApplications_NoApplications_NoContentObjectResult()
        {
            // Arrange
            IQueryable<JobApplication> expectedjobApplications = new List<JobApplication>().AsQueryable();

            _jobApplicationService.GetAllApplications().Returns(expectedjobApplications);

            // Act
            IActionResult result = await _jobApplicationsController.ListAllApplications();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void AddNewApplication_ApplicationDoesNotExist_CreatedResultWithJobApplication()
        {
            // Arrange
            var jobApplication = new JobApplication
            {
                Id = 1,
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                Status = Status.Interview
            };

            // Act
            IActionResult result = await _jobApplicationsController.AddNewApplication(jobApplication);

            // Assert
            await _jobApplicationService.Received().AddNewApplication(jobApplication);

            Assert.IsType<CreatedAtActionResult>(result);
            var createdResult = (CreatedAtActionResult)result;
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
            IActionResult result = await _jobApplicationsController.AddNewApplication(jobApplication);

            // Assert
            await _jobApplicationService.Received().AddNewApplication(jobApplication);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetApplication_ApplicationExists_OkObjectResultWithJobApplications()
        {
            // Arrange
            JobApplication expectedjobApplication = _expectedjobApplications.ToArray()[0];
            _jobApplicationService.GetApplication("Company 1").Returns(expectedjobApplication);

            // Act
            IActionResult result = await _jobApplicationsController.GetApplication("Company 1");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(expectedjobApplication, okResult.Value);
        }

        [Fact]
        public async Task GetApplication_ApplicationDoesNotExist_NotFoundObjectResult()
        {
            // Arrange
            _jobApplicationService.GetApplication("Company 1").Returns((JobApplication)null);

            // Act
            IActionResult result = await _jobApplicationsController.GetApplication("Company 1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetApplication_ServiceThrowsException_BadRequestResult()
        {
            // Arrange
            _jobApplicationService.GetApplication("Company 1").Throws(new InvalidOperationException());

            // Act
            IActionResult result = await _jobApplicationsController.GetApplication("Company 1");

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }


        [Fact]
        public async Task UpdateApplication__CallsServiceUpdateApplication()
        {
            // Arrange
            JobApplication jobApplication = _expectedjobApplications.First();

            // Act
            IActionResult result = await _jobApplicationsController.UpdateApplication(jobApplication);

            // Assert
            await _jobApplicationService.Received().UpdateApplication(jobApplication);

            Assert.IsType<OkResult>(result);
        }
    }
}
