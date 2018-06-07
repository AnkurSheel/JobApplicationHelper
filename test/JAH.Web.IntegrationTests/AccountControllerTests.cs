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
        private readonly Uri _baseUri;

        private readonly ClientFixture _fixture;

        public AccountControllerTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _baseUri = new Uri(_fixture.WebClient.BaseAddress, "account/");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task Register_Succeeds_RedirectsToLogin()
        {
            // Arrange
            var credentials = new CredentialModel { Email = "user@test.com", Password = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            var response = await _fixture.WebClient.PostAsync(_baseUri, stringContent).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/Auth/Login", response.Headers.Location.OriginalString);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fixture.EmptyDatabase();
            }
        }
    }
}
