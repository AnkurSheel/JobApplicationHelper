using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using JAH.Api.Controllers;
using JAH.Data.Entities;
using JAH.DomainModels;

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

        private readonly SignInManager<JobApplicationUser> _signInManager;

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
            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<JobApplicationUser>>();
            var signinManagerLogger = Substitute.For<ILogger<SignInManager<JobApplicationUser>>>();
            var schemes = Substitute.For<IAuthenticationSchemeProvider>();

            var userManager = Substitute.For<UserManager<JobApplicationUser>>(store,
                                                                           optionsAccessor,
                                                                           passwordHasher,
                                                                           userValidators,
                                                                           passwordValidators,
                                                                           keyNormalizer,
                                                                           errors,
                                                                           services,
                                                                           userManagerLogger);

            _signInManager =
                Substitute.ForPartsOf<SignInManager<JobApplicationUser>>(userManager,
                                                                         contextAccessor,
                                                                         claimsFactory,
                                                                         optionsAccessor,
                                                                         signinManagerLogger,
                                                                         schemes);

            var logger = Substitute.For<ILogger<AuthController>>();

            _authController = new AuthController(_signInManager, logger);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void Login_Succeeds_Ok()
        {
            // Arrange
            var model = new CredentialModel();
            _signInManager.WhenForAnyArgs(x => x.PasswordSignInAsync(string.Empty, string.Empty, true, true)).DoNotCallBase();
            _signInManager.PasswordSignInAsync(string.Empty, string.Empty, true, true)
                          .ReturnsForAnyArgs(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

            // Act
            Task<IActionResult> result = _authController.Login(model);

            // Assert
            Assert.IsType<OkResult>(result.Result);
        }

        [Fact]
        public void Login_Fails_BadRequestObjectResult()
        {
            // Arrange
            var model = new CredentialModel();
            _signInManager.WhenForAnyArgs(x => x.PasswordSignInAsync(string.Empty, string.Empty, true, true)).DoNotCallBase();
            _signInManager.PasswordSignInAsync(string.Empty, string.Empty, true, true)
                          .ReturnsForAnyArgs(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

            // Act
            Task<IActionResult> result = _authController.Login(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void IsSignedIn_UserNotSignedIn_OkObjectResultWithFalseValue()
        {
            // Arrange
            _signInManager.IsSignedIn(Arg.Any<ClaimsPrincipal>()).ReturnsForAnyArgs(true);

            // Act
            IActionResult result = _authController.SignedIn();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.True(okResult.Value as bool?);
        }

        [Fact]
        public void IsSignedIn_UserSignedIn_OkObjectResultWithTrueValue()
        {
            // Arrange
            _signInManager.IsSignedIn(Arg.Any<ClaimsPrincipal>()).ReturnsForAnyArgs(false);

            // Act
            IActionResult result = _authController.SignedIn();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.False(okResult.Value as bool?);
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
