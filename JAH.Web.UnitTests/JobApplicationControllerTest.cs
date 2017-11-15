using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JAH.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace JAH.Web.UnitTests
{
    public class JobApplicationControllerTest
    {
        private readonly JobApplicationController _jobApplicationController;
        private readonly FakeHttpMessageHandler _httpMessageHandler;
        private HttpRequestMessage _httpRequestMessage;

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
            const string expectedJson = "[{\"name\":\"Company 1\"},{\"name\":\"Company 2\"},{\"name\":\"Company 3\"}]";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(expectedJson) };

            _httpMessageHandler.Send(_httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(expectedJson, okResult.Value);
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
            Assert.Equal(expectedJson, okResult.Value);
        }
    }
}
