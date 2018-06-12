using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JAH.Api.Controllers;
using JAH.Data.Entities;
using JAH.DomainModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using Xunit;

namespace JAH.Api.UnitTests
{
    public class AccountApiControllerTest : IDisposable
    {
        private readonly AccountController _accountController;

        private readonly UserManager<JobApplicationUser> _userManager;

        public AccountApiControllerTest()
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

            _userManager = Substitute.ForPartsOf<UserManager<JobApplicationUser>>(store,
                                                                                  optionsAccessor,
                                                                                  passwordHasher,
                                                                                  userValidators,
                                                                                  passwordValidators,
                                                                                  keyNormalizer,
                                                                                  errors,
                                                                                  services,
                                                                                  userManagerLogger);

            var logger = Substitute.For<ILogger<AccountController>>();

            _accountController = new AccountController(_userManager, logger);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void Register_Succeeds_Ok()
        {
            // Arrange
            var model = new RegisterModel();
            _userManager.CreateAsync(Arg.Any<JobApplicationUser>(), string.Empty).ReturnsForAnyArgs(IdentityResult.Success);

            // Act
            Task<IActionResult> result = _accountController.Register(model);

            // Assert
            Assert.IsType<OkResult>(result.Result);
        }

        [Fact]
        public void Register_Fails_BadRequestObjectResult()
        {
            // Arrange
            var model = new RegisterModel();
            _userManager.CreateAsync(Arg.Any<JobApplicationUser>(), string.Empty).ReturnsForAnyArgs(IdentityResult.Failed(null));

            // Act
            Task<IActionResult> result = _accountController.Register(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _accountController?.Dispose();
                _userManager?.Dispose();
            }
        }
    }
}
