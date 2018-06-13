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
            _userManager = Substitute.For<UserManager<JobApplicationUser>>(Substitute.For<IUserStore<JobApplicationUser>>(),
                                                                           Substitute.For<IOptions<IdentityOptions>>(),
                                                                           Substitute.For<IPasswordHasher<JobApplicationUser>>(),
                                                                           Substitute.For<IEnumerable<IUserValidator<JobApplicationUser>>>(),
                                                                           Substitute.For<IEnumerable<IPasswordValidator<JobApplicationUser>>>(),
                                                                           Substitute.For<ILookupNormalizer>(),
                                                                           Substitute.For<IdentityErrorDescriber>(),
                                                                           Substitute.For<IServiceProvider>(),
                                                                           Substitute.For<ILogger<UserManager<JobApplicationUser>>>());

            _tokenGenerator = Substitute.For<ITokenGenerator>();

            _accountManagerService = new AccountManagerService(_userManager, _tokenGenerator);
        }

        public static IEnumerable<object[]> GetRoles()
        {
            yield return new object[] { new List<string>() };
            yield return new object[] { new List<string> { "role1", "role2" } };
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

        [Theory]
        [MemberData(nameof(GetRoles))]
        public async Task GetTokenWithClaimsPrincipal_ValidUser_AddsDefaultClaims(List<string> roles)
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(true));
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

        [Theory]
        [MemberData(nameof(GetRoles))]
        public async Task GetJwtToken_ValidUser_AddsRolesClaim(List<string> roles)
        {
            // Arrange
            var model = new LoginModel { Email = "user@test.com", Password = "password" };
            var user = new JobApplicationUser(model.Email);
            _userManager.FindByEmailAsync(model.Email).Returns(Task.FromResult(user));
            _userManager.CheckPasswordAsync(user, model.Password).Returns(Task.FromResult(true));
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
