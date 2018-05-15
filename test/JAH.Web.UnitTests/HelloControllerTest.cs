using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using JAH.Web.Controllers;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using Xunit;

namespace JAH.Web.UnitTests
{
    public class HelloControllerTest : IDisposable
    {
        private readonly HelloController _helloController;

        private readonly FakeHttpMessageHandler _httpMessageHandler;

        public HelloControllerTest()
        {
            _httpMessageHandler = Substitute.ForPartsOf<FakeHttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandler) { BaseAddress = new Uri("http://localhost/") };

            _helloController = new HelloController(httpClient);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Theory]
        [InlineData("name 1", "Hello name 1")]
        [InlineData("name 2", "Hello name 2")]
        public async Task Greet_Name_Greeting(string name, string expected)
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(expected) };

            var httpRequestMessage = new HttpRequestMessage();
            _httpMessageHandler.WhenForAnyArgs(x => x.Send(httpRequestMessage)).DoNotCallBase();
            _httpMessageHandler.Send(httpRequestMessage).ReturnsForAnyArgs(httpResponseMessage);

            // Act
            IActionResult result = await _helloController.Greet(name).ConfigureAwait(false);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            var actual = okResult.Value as string;
            Assert.Equal(expected, actual);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _helloController?.Dispose();
                _httpMessageHandler?.Dispose();
            }
        }
    }
}
