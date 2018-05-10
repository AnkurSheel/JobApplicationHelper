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
        private const string UriBasePath = "/account";

        private readonly ClientFixture _fixture;

        /// <inheritdoc />
        public AccountControllerTests(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
            _fixture.EmptyDatabase();
        }

        [Fact]
        public async Task Login_Fails_RedirectsToJobApplications()
        {
            // Arrange
            var credentials = new CredentialModel { UserName = "username", Password = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            var response = await _fixture.WebClient.PostAsync($"{UriBasePath}/login", stringContent);

            // Assert
            var responseData = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("UserName/Password Not found", responseData);
        }

        [Fact]
        public async Task Login_Succeeds_RedirectsToJobApplications()
        {
            // Arrange
            var userManager = _fixture.Services.GetRequiredService<UserManager<JobApplicationUser>>();
            var credentials = new CredentialModel { UserName = "username", Password = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            var user = new JobApplicationUser(credentials.UserName);
            await userManager.CreateAsync(user, credentials.Password);

            // Act
            var response = await _fixture.WebClient.PostAsync($"{UriBasePath}/login", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/JobApplications", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Logout_Succeeds_RedirectsToLoginPage()
        {
            // Arrange
            _fixture.SetupAuthentication();

            // Act
            var response = await _fixture.WebClient.PostAsync($"{UriBasePath}/logout", null);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/Account/Login", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Register_Succeeds_RedirectsToLogin()
        {
            // Arrange
            var credentials = new CredentialModel { UserName = "username", Password = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            var response = await _fixture.WebClient.PostAsync($"{UriBasePath}/register", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/JobApplications", response.Headers.Location.OriginalString);
        }
    }
}