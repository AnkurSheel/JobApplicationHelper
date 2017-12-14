using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace JAH.Web.IntegrationTests
{
    public class HelloControllerTest : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;

        public HelloControllerTest(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("name 1", "Hello name 1")]
        [InlineData("name 2", "Hello name 2")]
        public async Task GetAsync_Name_Greeting(string name, string expected)
        {
            // Arrange

            // Act
            HttpResponseMessage response = await _fixture.WebClient.GetAsync($"/hello/{name}");

            // Assert
            response.EnsureSuccessStatusCode();
            string actual = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(expected, actual);
        }
    }
}
