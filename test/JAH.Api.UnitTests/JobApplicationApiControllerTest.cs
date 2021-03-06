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
    public class JobApplicationApiControllerTest : IDisposable
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
                                                                               new JobApplication
                                                                               {
                                                                                   CompanyName = "Company 1",
                                                                                   ApplicationDate =
                                                                                       new DateTime(2017, 11, 13),
                                                                                   Status = Status.Interview
                                                                               },
                                                                               new JobApplication
                                                                               {
                                                                                   CompanyName = "Company 2",
                                                                                   ApplicationDate =
                                                                                       new DateTime(2017, 11, 14),
                                                                                   Status = Status.Applied
                                                                               },
                                                                               new JobApplication
                                                                               {
                                                                                   CompanyName = "Company 3",
                                                                                   ApplicationDate =
                                                                                       new DateTime(2017, 11, 14),
                                                                                   Status = Status.Offer
                                                                               }
                                                                           });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task Get_MultipleApplications_OkObjectResultWithAListOfJobApplications()
        {
            // Arrange
            _jobApplicationService.GetAllApplications().Returns(_expectedjobApplications);

            // Act
            IActionResult result = await _jobApplicationsController.Get().ConfigureAwait(false);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(_expectedjobApplications, okResult.Value);
        }

        [Fact]
        public async Task Get_NoApplications_NoContentObjectResult()
        {
            // Arrange
            IQueryable<JobApplication> expectedjobApplications = new List<JobApplication>().AsQueryable();

            _jobApplicationService.GetAllApplications().Returns(expectedjobApplications);

            // Act
            IActionResult result = await _jobApplicationsController.Get().ConfigureAwait(false);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void Post_ApplicationDoesNotExist_CreatedResultWithJobApplication()
        {
            // Arrange
            var jobApplication = new JobApplication
                                 {
                                     CompanyName = "Company 1",
                                     ApplicationDate = new DateTime(2017, 11, 13),
                                     Status = Status.Interview
                                 };
            _jobApplicationService.AddNewApplication(jobApplication).Returns(jobApplication);

            // Act
            IActionResult result = await _jobApplicationsController.Post(jobApplication).ConfigureAwait(false);

            // Assert
            await _jobApplicationService.Received().AddNewApplication(jobApplication).ConfigureAwait(false);

            Assert.IsType<CreatedAtRouteResult>(result);
            var createdResult = (CreatedAtRouteResult)result;
            Assert.Equal(jobApplication, createdResult.Value);
        }

        [Fact]
        public async Task Post_ApplicationExists_BadRequestResult()
        {
            // Arrange
            var jobApplication = new JobApplication
                                 {
                                     CompanyName = "Company 1",
                                     ApplicationDate = new DateTime(2017, 11, 13),
                                     Status = Status.Interview
                                 };

            _jobApplicationService.AddNewApplication(jobApplication).Throws<ArgumentException>();

            // Act
            IActionResult result = await _jobApplicationsController.Post(jobApplication).ConfigureAwait(false);

            // Assert
            await _jobApplicationService.Received().AddNewApplication(jobApplication).ConfigureAwait(false);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Get_ApplicationExists_OkObjectResultWithJobApplications()
        {
            // Arrange
            JobApplication expectedjobApplication = _expectedjobApplications.ToArray()[0];
            _jobApplicationService.GetApplication("Company 1").Returns(expectedjobApplication);

            // Act
            IActionResult result = _jobApplicationsController.Get("Company 1");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(expectedjobApplication, okResult.Value);
        }

        [Fact]
        public void Get_ApplicationDoesNotExist_NotFoundObjectResult()
        {
            // Arrange
            var companyName = "Company 1";
            _jobApplicationService.GetApplication(companyName).Returns((JobApplication)null);

            // Act
            IActionResult result = _jobApplicationsController.Get(companyName);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.Equal($"Company with Name \"{companyName}\" was not found", notFoundResult.Value);
        }

        [Fact]
        public void Get_ServiceThrowsException_BadRequestResult()
        {
            // Arrange
            _jobApplicationService.GetApplication("Company 1").Throws(new InvalidOperationException());

            // Act
            IActionResult result = _jobApplicationsController.Get("Company 1");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Put_ApplicationExists_OkObjectResultWithJobApplication()
        {
            // Arrange
            JobApplication expectedjobApplication = _expectedjobApplications.ToArray()[0];
            string companyName = expectedjobApplication.CompanyName;
            _jobApplicationService.UpdateApplication(companyName, expectedjobApplication).Returns(expectedjobApplication);

            // Act
            IActionResult result = await _jobApplicationsController.Put(companyName, expectedjobApplication).ConfigureAwait(false);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(expectedjobApplication, okResult.Value);
        }

        [Fact]
        public async Task Put_ApplicationDoesNotExist_NotFoundObjectResult()
        {
            // Arrange
            JobApplication jobApplication = _expectedjobApplications.First();
            string companyName = jobApplication.CompanyName;

            // Act
            IActionResult result = await _jobApplicationsController.Put(companyName, jobApplication).ConfigureAwait(false);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.Equal($"Company with Name \"{companyName}\" was not found", notFoundResult.Value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _jobApplicationsController?.Dispose();
            }
        }
    }
}
