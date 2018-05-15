using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace JAH.Web.IntegrationTests
{
    public class HelloControllerTest : IClassFixture<ClientFixture>
    {
        private readonly Uri _baseUri;

        private readonly ClientFixture _fixture;

        public HelloControllerTest(ClientFixture fixture)
        {
            _fixture = fixture;
            _baseUri = new Uri(_fixture.WebClient.BaseAddress, "hello/");
        }

        [Theory]
        [InlineData("name 1", "Hello name 1")]
        [InlineData("name 2", "Hello name 2")]
        public async Task GetAsync_NameAndAuthorized_Greeting(string name, string expected)
        {
            // Arrange
            _fixture.SetupAuthentication();

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync(new Uri(_baseUri, $"{name}")).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            string actual = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetAsync_NameAndNotAuthorized_Greeting()
        {
            // Arrange
            _fixture.ClearAuthentication();

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync(new Uri(_baseUri, $"name")).ConfigureAwait(false);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
