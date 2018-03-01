using System;
using System.Collections.Generic;
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
    public class AuthApiControllerTest
    {
        private readonly AuthController _authController;
        private readonly SignInManager<JobApplicationUser> _signInManager;
        private readonly UserManager<JobApplicationUser> _userManager;
        private readonly ILogger<AuthController> _authControllerLogger;

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

            _userManager = Substitute.ForPartsOf<UserManager<JobApplicationUser>>(store,
                                                                                  optionsAccessor,
                                                                                  passwordHasher,
                                                                                  userValidators,
                                                                                  passwordValidators,
                                                                                  keyNormalizer,
                                                                                  errors,
                                                                                  services,
                                                                                  userManagerLogger);

            _signInManager =
                Substitute.ForPartsOf<SignInManager<JobApplicationUser>>(_userManager,
                                                                         contextAccessor,
                                                                         claimsFactory,
                                                                         optionsAccessor,
                                                                         signinManagerLogger,
                                                                         schemes);
            _authControllerLogger = Substitute.For<ILogger<AuthController>>();

            _authController = new AuthController(_signInManager, _userManager, _authControllerLogger);
        }

        [Fact]
        public void Register_Succeeds_Ok()
        {
            // Arrange
            var model = new CredentialModel();
            _userManager.CreateAsync(Arg.Any<JobApplicationUser>(), string.Empty).ReturnsForAnyArgs(IdentityResult.Success);

            // Act
            Task<IActionResult> result = _authController.Register(model);

            // Assert
            Assert.IsType<OkResult>(result.Result);
        }

        [Fact]
        public void Register_Fails_BadRequestObjectResult()
        {
            // Arrange
            var model = new CredentialModel();
            _userManager.CreateAsync(Arg.Any<JobApplicationUser>(), string.Empty).ReturnsForAnyArgs(IdentityResult.Failed(null));

            // Act
            Task<IActionResult> result = _authController.Register(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
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
    }
}
