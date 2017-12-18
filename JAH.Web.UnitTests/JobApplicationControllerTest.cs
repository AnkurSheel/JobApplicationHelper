using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JAH.DomainModels;
using JAH.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace JAH.Web.UnitTests
{
    public class JobApplicationControllerTest
    {
        private readonly JobApplicationController _jobApplicationController;
        private readonly FakeHttpMessageHandler _httpMessageHandler;
        private readonly HttpRequestMessage _httpRequestMessage;

        public JobApplicationControllerTest()
        {
            _httpMessageHandler = Substitute.ForPartsOf<FakeHttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandler) { BaseAddress = new Uri("http://localhost/api/") };
            _httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(_httpRequestMessage)).DoNotCallBase();

            _jobApplicationController = new JobApplicationController(httpClient);
        }

        [Fact]
        public async Task ListAllApplications_MultipleApplications_ViewResultWithAllJobApplications()
        {
            // Arrange
            var expectedJobApplications = new List<JobApplication>
            {
                new JobApplication { CompanyName = "Company 1", ApplicationDate = new DateTime(2017, 11, 13), Status = Status.Interview },
                new JobApplication { CompanyName = "Company 2", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Applied },
                new JobApplication { CompanyName = "Company 3", ApplicationDate = new DateTime(2017, 11, 14), Status = Status.Offer }
            };

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedJobApplications))
            };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationController.ListAllApplications();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.Equal(expectedJobApplications, viewResult.Model);
        }

        [Fact]
        public async Task ListAllApplications_NoApplications_EmptyViewResult()
        {
            // Arrange
            const string expectedJson = "";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(expectedJson) };

            var httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(httpRequestMessage)).DoNotCallBase();
            _httpMessageHandler.Send(httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationController.ListAllApplications();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.Equal(new List<JobApplication>(), viewResult.Model);
        }

        [Fact]
        public async void AddNewApplication_ApplicationExists_501()
        {
            // Arrange
            var jobApplication = new JobApplication
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                Status = Status.Interview
            };
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonConvert.SerializeObject("{Duplicate Name\": [\"Name Company 1 already exists.\"]}"))
            };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);
            // Act
            IActionResult result = await _jobApplicationController.AddNewApplication(jobApplication);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCodeResult = (StatusCodeResult)result;
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        [Fact]
        public async void AddNewApplication_ApplicationDoesNotExists_RedirectToActionObjectResult()
        {
            // Arrange
            var jobApplication = new JobApplication
            {
                CompanyName = "Company 1",
                ApplicationDate = new DateTime(2017, 11, 13),
                Status = Status.Interview
            };
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(jobApplication))
            };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationController.AddNewApplication(jobApplication);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectToActionResult = (RedirectToActionResult)result;
            Assert.Equal("ListAllApplications", redirectToActionResult.ActionName);
        }
    }
}
