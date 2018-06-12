using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using JAH.Api.Controllers;
using JAH.Data.Entities;
using JAH.DomainModels;
using JAH.Helper;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using Xunit;

namespace JAH.Api.UnitTests
{
    public class AuthApiControllerTest : IDisposable
    {
        private readonly AuthController _authController;

        private readonly UserManager<JobApplicationUser> _userManager;

        private readonly ITokenGenerator _tokenGenerator;

        private readonly HttpContext _httpContext;

        public AuthApiControllerTest()
        {
            var store = Substitute.For<IUserStore<JobApplicationUser>>();
            var optionsAccessor = Substitute.For<IOptions<IdentityOptions>>();
            var passwordHasher = Substitute.For<IPasswordHasher<JobApplicationUser>>();
            var userValidators = Substitute.For<IEnumerable<IUserValidator<JobApplicationUser>>>();
            var passwordValidators = Substitute.For<IEnumerable<IPasswordValidator<JobApplicationUser>>>();
            var keyNormalizer = Substitute.For<ILookupNormalizer>();
            var errors = Substitute.For<IdentityErrorDescriber>();
            var services = Substitute.For<IServiceProvider>();
            var userManagerLogger = Substitute.For<ILogger<UserManager<JobApplicationUser>>>();

            _userManager = Substitute.For<UserManager<JobApplicationUser>>(store,
                                                                           optionsAccessor,
                                                                           passwordHasher,
                                                                           userValidators,
                                                                           passwordValidators,
                                                                           keyNormalizer,
                                                                           errors,
                                                                           services,
                                                                           userManagerLogger);

            _tokenGenerator = Substitute.For<ITokenGenerator>();

            _httpContext = Substitute.For<HttpContext>();

            var authServiceMock = Substitute.For<IAuthenticationService>();
            var serviceProviderMock = Substitute.For<IServiceProvider>();

            authServiceMock.SignInAsync(Arg.Any<HttpContext>(), Arg.Any<string>(), Arg.Any<ClaimsPrincipal>(), Arg.Any<AuthenticationProperties>())
                           .Returns(Task.FromResult((object)null));
            serviceProviderMock.GetService(typeof(IAuthenticationService)).Returns(authServiceMock);

            _httpContext.RequestServices.Returns(serviceProviderMock);

            var logger = Substitute.For<ILogger<AuthController>>();
            _authController =
                new AuthController(_userManager, _tokenGenerator, logger)
                {
                    ControllerContext = new ControllerContext { HttpContext = _httpContext }
                };
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
            var email = "user@test.com";
            var password = "password";
            var model = new LoginModel { Email = email, Password = password };
            var user = new JobApplicationUser(email);
            _userManager.FindByEmailAsync(email).Returns(user);
            _userManager.CheckPasswordAsync(user, password).Returns(true);
            _tokenGenerator.GenerateAccessTokenWithClaimsPrincipal(string.Empty, null).ReturnsForAnyArgs(new TokenWithClaimsPrincipal());

            // Act
            Task<IActionResult> result = _authController.Login(model);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void Login_InvalidModel_BadRequestObjectResult()
        {
            // Arrange
            var email = "user";
            var model = new LoginModel { Email = email };

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
