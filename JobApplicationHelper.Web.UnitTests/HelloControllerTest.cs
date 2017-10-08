using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JobApplicationHelper.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace JobApplicationHelper.Web.UnitTests
{
    public class HelloControllerTest
    {
        private readonly HelloController _helloController;
        private readonly FakeHttpMessageHandler _httpMessageHandler;

        public HelloControllerTest()
        {
            _httpMessageHandler = Substitute.ForPartsOf<FakeHttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandler) { BaseAddress = new Uri("http://localhost/api/") };

            _helloController = new HelloController(httpClient);
        }

        [Fact]
        public async Task ShouldReturnOkObjectResultWithAGreeting()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("Hello ankur") };

            var httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(httpRequestMessage)).DoNotCallBase();
            _httpMessageHandler.Send(httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            var result = await _helloController.Greet("ankur");

            // Assert
            Assert.IsType(typeof(OkObjectResult), result);
            var okResult = (OkObjectResult)result;
            Assert.Equal("Hello ankur", okResult.Value);
        }
    }
}
