﻿using System;
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
        public async Task Login_Fails_RedirectsToJobApplications()
        {
            // Arrange
            var credentials = new CredentialModel { UserName = "username", Password = "password" };
            var stringContent = new StringContent(credentials.ToUrl(), Encoding.UTF8, "application/x-www-form-urlencoded");

            // Act
            HttpResponseMessage response = await _fixture.WebClient.PostAsync(new Uri(_baseUri, "login"), stringContent).ConfigureAwait(false);

            // Assert
            string responseData = response.Content.ReadAsStringAsync().Result;
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
            await userManager.CreateAsync(user, credentials.Password).ConfigureAwait(false);

            // Act
            HttpResponseMessage response = await _fixture.WebClient.PostAsync(new Uri(_baseUri, "login"), stringContent).ConfigureAwait(false);

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
            HttpResponseMessage response = await _fixture.WebClient.PostAsync(new Uri(_baseUri, "logout"), null).ConfigureAwait(false);

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
            HttpResponseMessage response = await _fixture.WebClient.PostAsync(new Uri(_baseUri, "register"), stringContent).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/JobApplications", response.Headers.Location.OriginalString);
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
