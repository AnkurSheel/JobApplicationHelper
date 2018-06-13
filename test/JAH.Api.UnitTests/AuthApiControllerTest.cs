using System;
using System.Security.Claims;
using System.Threading.Tasks;

using JAH.Api.Controllers;
using JAH.DomainModels;
using JAH.Helper;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Xunit;

namespace JAH.Api.UnitTests
{
    public class AuthApiControllerTest : IDisposable
    {
        private readonly AuthController _authController;

        private readonly HttpContext _httpContext;

        private readonly IAccountManagerService _accountManagerService;

        public AuthApiControllerTest()
        {
            _accountManagerService = Substitute.For<IAccountManagerService>();

            _httpContext = Substitute.For<HttpContext>();

            var authServiceMock = Substitute.For<IAuthenticationService>();
            var serviceProviderMock = Substitute.For<IServiceProvider>();

            authServiceMock.SignInAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<ClaimsPrincipal>(), Arg.Any<AuthenticationProperties>())
                           .Returns(Task.FromResult((object)null));
            serviceProviderMock.GetService(typeof(IAuthenticationService)).Returns(authServiceMock);

            _httpContext.RequestServices.Returns(serviceProviderMock);

            var logger = Substitute.For<ILogger<AuthController>>();
            _authController =
                new AuthController(_accountManagerService, logger) { ControllerContext = new ControllerContext { HttpContext = _httpContext } };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void Login_Succeeds_OkObjectResult()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            _accountManagerService.GetTokenWithClaimsPrincipal(model).ReturnsForAnyArgs(new TokenWithClaimsPrincipal());

            // Act
            Task<IActionResult> result = _authController.Login(model);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Login_InvalidModel_BadRequestObjectResult()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            _accountManagerService.GetTokenWithClaimsPrincipal(model).ReturnsForAnyArgs(null as TokenWithClaimsPrincipal);

            // Act
            Task<IActionResult> result = _authController.Login(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void IsSignedIn_UserNotSignedIn_OkObjectResultWithFalseValue()
        {
            // Arrange
            _httpContext.User.Identity.IsAuthenticated.Returns(false);

            // Act
            var result = _authController.SignedIn();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.False(okResult.Value as bool?);
        }

        [Fact]
        public void IsSignedIn_UserSignedIn_OkObjectResultWithTrueValue()
        {
            // Arrange
            _httpContext.User.Identity.IsAuthenticated.Returns(true);

            // Act
            var result = _authController.SignedIn();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.True(okResult.Value as bool?);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _authController?.Dispose();
            }
        }
    }
}
