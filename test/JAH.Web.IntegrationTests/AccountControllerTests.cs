using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using JAH.Data.Entities;
using JAH.DomainModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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
            var credentials = new RegisterModel { Email = "user@test.com", Password = "password", ConfirmPassword = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            var response = await _fixture.WebClient.PostAsync(_baseUri, stringContent).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/Auth/Login", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Register_DuplicateAccount_ShowsError()
        {
            // Arrange
            var userManager = _fixture.Services.GetRequiredService<UserManager<JobApplicationUser>>();
            var credentials = new RegisterModel { Email = "username@test.com", Password = "password", ConfirmPassword = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            var user = new JobApplicationUser(credentials.Email);
            await userManager.CreateAsync(user, credentials.Password).ConfigureAwait(false);

            // Act
            var response = await _fixture.WebClient.PostAsync(_baseUri, stringContent).ConfigureAwait(false);

            // Assert
            var responseData = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("Email 'username@test.com' is already taken.", responseData);
        }

        [Fact]
        public async Task Register_PasswordAndConfirmationDoesNotMatch_ShowsError()
        {
            // Arrange
            var userManager = _fixture.Services.GetRequiredService<UserManager<JobApplicationUser>>();
            var credentials = new RegisterModel { Email = "username@test.com", Password = "password", ConfirmPassword = "failure" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            var user = new JobApplicationUser(credentials.Email);
            await userManager.CreateAsync(user, credentials.Password).ConfigureAwait(false);

            // Act
            var response = await _fixture.WebClient.PostAsync(_baseUri, stringContent).ConfigureAwait(false);

            // Assert
            var responseData = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("The password and confirmation password do not match.", responseData);
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
