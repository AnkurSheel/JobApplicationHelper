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
        public async Task ShouldReturnJsonWithAllJobApplications()
        {
            // Arrange
            var expectedJobApplications = new List<JobApplication>
            {
                new JobApplication {Name = "Company 1", StartDate = new DateTime(2017, 11, 13), Status = Status.None},
                new JobApplication {Name = "Company 2", StartDate = new DateTime(2017, 11, 14), Status = Status.Applied},
                new JobApplication {Name = "Company 3", StartDate = new DateTime(2017, 11, 14), Status = Status.Offer}
            };

            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(expectedJobApplications)) };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(expectedJobApplications, okResult.Value);
        }

        [Fact]
        public async Task ShouldReturnEmptyJsonWhenNoJobApplicationsExist()
        {
            // Arrange
            const string expectedJson = "";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(expectedJson) };

            var httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(httpRequestMessage)).DoNotCallBase();
            _httpMessageHandler.Send(httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Null(okResult.Value);
        }
    }
}
