using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JAH.DomainModels;
using Xunit;

namespace JAH.Web.IntegrationTests
{
    public class AccountControllerTests : IClassFixture<ClientFixture>, IDisposable
    {
        private const string UriBasePath = "/account";
        private readonly ClientFixture _fixture;

        /// <inheritdoc />
        public AccountControllerTests(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
            _fixture.DetachAllEntities();
            _fixture.JobApplicationDbContext.Database.EnsureDeleted();
        }

        [Fact]
        public async Task Register_Succeeds_RedirectsToLogin()
        {
            // Arrange
            var credentials = new CredentialModel { UserName = "username", Password = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseMessage response = await _fixture.WebClient.PostAsync($"{UriBasePath}/register", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/JobApplications", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Login_Succeeds_RedirectsToJobApplications()
        {
            // Arrange
            var credentials = new CredentialModel { UserName = "username", Password = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            // TODO: This should be replaced by actually making an entry in the database
            await _fixture.WebClient.PostAsync($"{UriBasePath}/register", stringContent);

            // Act
            HttpResponseMessage response = await _fixture.WebClient.PostAsync($"{UriBasePath}/login", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/JobApplications", response.Headers.Location.OriginalString);
        }
    }
}
