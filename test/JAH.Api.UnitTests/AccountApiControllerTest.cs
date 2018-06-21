using System;
using System.Threading.Tasks;

using JAH.Api.Controllers;
using JAH.DomainModels;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using Xunit;

namespace JAH.Api.UnitTests
{
    public class AccountApiControllerTest : IDisposable
    {
        private readonly AccountController _accountController;

        private readonly IAccountManagerService _accountManagerService;

        public AccountApiControllerTest()
        {
            _accountManagerService = Substitute.For<IAccountManagerService>();

            _accountController = new AccountController(_accountManagerService);
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
            var model = new RegisterModel { Email = "user@test.com", Password = "password" };
            _accountManagerService.RegisterUser(model).Returns(IdentityResult.Success);

            // Act
            Task<IActionResult> result = _accountController.Register(model);

            // Assert
            Assert.IsType<OkResult>(result.Result);
        }

        [Fact]
        public void Register_Fails_BadRequestObjectResult()
        {
            // Arrange
            var model = new RegisterModel { Email = "user@test.com", Password = "password" };
            _accountManagerService.RegisterUser(model).Returns(IdentityResult.Failed(null));

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
                _accountManagerService?.Dispose();
            }
        }
    }
}
