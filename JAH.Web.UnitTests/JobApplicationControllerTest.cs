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

        public JobApplicationControllerTest()
        {
            _httpMessageHandler = Substitute.ForPartsOf<FakeHttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandler) { BaseAddress = new Uri("http://localhost/api/") };

            _jobApplicationController = new JobApplicationController(httpClient);
        }

        [Fact]
        public async Task ShouldReturnOkObjectResultWithAllJobApplications()
        {
            // Arrange
            var expectedJson = "[{\"name\":\"Company 1\"},{\"name\":\"Company 2\"},{\"name\":\"Company 3\"}]";
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(expectedJson) };

            var httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(httpRequestMessage)).DoNotCallBase();
            _httpMessageHandler.Send(httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            var result = await _jobApplicationController.List();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(expectedJson, okResult.Value);
        }
    }
}
