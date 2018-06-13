using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using JAH.Data.Entities;
using JAH.DomainModels;
using JAH.Helper;
using JAH.Helper.Constants;
using JAH.Services.Interfaces;
using JAH.Services.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using Xunit;

namespace JAH.Services.UnitTests
{
    public class AccountManagerServiceTests : IDisposable
    {
        private readonly UserManager<JobApplicationUser> _userManager;

        private readonly IAccountManagerService _accountManagerService;

        private readonly ITokenGenerator _tokenGenerator;

        public AccountManagerServiceTests()
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

            _accountManagerService = new AccountManagerService(_userManager, _tokenGenerator);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void RegisterUser_CallsUserManagerCreateAsync()
        {
            // Arrange
            var model = new RegisterModel { Email = "user@test.com", Password = "password" };

            // Act
            _accountManagerService.RegisterUser(model);

            // Assert
            Received.InOrder(async () => { await _userManager.CreateAsync(Arg.Any<JobApplicationUser>(), model.Password).ConfigureAwait(false); });
        }

        [Fact]
        public async Task GetTokenWithClaimsPrincipal_UserNotFound_ReturnsNull()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult<JobApplicationUser>(null));

            // Act
            var result = await _accountManagerService.GetTokenWithClaimsPrincipal(model).ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTokenWithClaimsPrincipal_IncorrectPassword_ReturnsNull()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(false));

            // Act
            var result = await _accountManagerService.GetTokenWithClaimsPrincipal(model).ConfigureAwait(false);

            // Assert
            Received.InOrder(async () =>
            {
                await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
            });
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTokenWithClaimsPrincipal_ValidUser_ReturnsTokenWithClaimPrincipals()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(true));
            var tokenWithClaimsPrincipal = new TokenWithClaimsPrincipal();
            _tokenGenerator.GetAccessTokenWithClaimsPrincipal(user.UserName, Arg.Any<IEnumerable<Claim>>()).Returns(tokenWithClaimsPrincipal);

            // Act
            var result = await _accountManagerService.GetTokenWithClaimsPrincipal(model).ConfigureAwait(false);

            // Assert
            Received.InOrder(async () =>
            {
                await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
            });
            Assert.Equal(tokenWithClaimsPrincipal, result);
        }

        [Fact]
        public async Task GetTokenWithClaimsPrincipal_ValidUser_AddsIdClaim()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(true));
            var claims = new List<Claim> { new Claim(JwtClaimIdentifiers.Id, user.Id) };

            // Act
            await _accountManagerService.GetTokenWithClaimsPrincipal(model).ConfigureAwait(false);

            // Assert
            _tokenGenerator.Received(1)
                           .GetAccessTokenWithClaimsPrincipal(user.UserName,
                                                              Arg.Is<IEnumerable<Claim>>(c => claims.SequenceEqual(c, new ClaimEqualityComparer())));
        }

        [Fact]
        public async Task GetTokenWithClaimsPrincipal_ValidUser_AddsRolesClaim()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(true));
            IList<string> roles = new List<string> { "testRole1", "testRole2" };
            _userManager.GetRolesAsync(user).Returns(roles);
            var claims = new List<Claim> { new Claim(JwtClaimIdentifiers.Id, user.Id) };
            claims.AddRange(roles.Select(role => new Claim(JwtClaimIdentifiers.Role, role)));

            // Act
            await _accountManagerService.GetTokenWithClaimsPrincipal(model).ConfigureAwait(false);

            // Assert
            _tokenGenerator.Received(1)
                           .GetAccessTokenWithClaimsPrincipal(user.UserName,
                                                              Arg.Is<IEnumerable<Claim>>(c => claims.SequenceEqual(c, new ClaimEqualityComparer())));
        }

        [Fact]
        public async Task GetJwtToken_UserNotFound_ReturnsNull()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult<JobApplicationUser>(null));

            // Act
            var result = await _accountManagerService.GetJwtToken(model).ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetJwtToken_IncorrectPassword_ReturnsNull()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(false));

            // Act
            var result = await _accountManagerService.GetJwtToken(model).ConfigureAwait(false);

            // Assert
            Received.InOrder(async () =>
            {
                await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
            });
            Assert.Null(result);
        }

        [Fact]
        public async Task GetJwtToken_ValidUser_ReturnsTokenWithClaimPrincipals()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(true));

            _tokenGenerator.GetJwtToken(user.UserName, Arg.Any<IEnumerable<Claim>>()).Returns("response");

            // Act
            var result = await _accountManagerService.GetJwtToken(model).ConfigureAwait(false);

            // Assert
            Received.InOrder(async () =>
            {
                await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
            });
            Assert.Equal("response", result);
        }

        [Fact]
        public async Task GetJwtToken_ValidUser_AddsIdClaim()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(true));
            var claims = new List<Claim> { new Claim(JwtClaimIdentifiers.Id, user.Id) };

            // Act
            await _accountManagerService.GetJwtToken(model).ConfigureAwait(false);

            // Assert
            _tokenGenerator.Received(1)
                           .GetJwtToken(user.UserName, Arg.Is<IEnumerable<Claim>>(c => claims.SequenceEqual(c, new ClaimEqualityComparer())));
        }

        [Fact]
        public async Task GetJwtToken_ValidUser_AddsRolesClaim()
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(true));
            IList<string> roles = new List<string> { "testRole1", "testRole2" };
            _userManager.GetRolesAsync(user).Returns(roles);
            var claims = new List<Claim> { new Claim(JwtClaimIdentifiers.Id, user.Id) };
            claims.AddRange(roles.Select(role => new Claim(JwtClaimIdentifiers.Role, role)));

            // Act
            await _accountManagerService.GetJwtToken(model).ConfigureAwait(false);

            // Assert
            _tokenGenerator.Received(1)
                           .GetJwtToken(user.UserName, Arg.Is<IEnumerable<Claim>>(c => claims.SequenceEqual(c, new ClaimEqualityComparer())));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _userManager?.Dispose();
                _accountManagerService?.Dispose();
            }
        }
    }
}
