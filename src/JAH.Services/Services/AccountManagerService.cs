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

using Microsoft.AspNetCore.Identity;

namespace JAH.Services.Services
{
    public class AccountManagerService : IAccountManagerService
    {
        private readonly UserManager<JobApplicationUser> _userManager;

        private readonly ITokenGenerator _tokenGenerator;

        public AccountManagerService(UserManager<JobApplicationUser> userManager, ITokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<IdentityResult> RegisterUser(RegisterModel model)
        {
            var jobApplicationUser = new JobApplicationUser(model.Email);

            var result = await _userManager.CreateAsync(jobApplicationUser, model.Password).ConfigureAwait(false);
            return result;
        }

        public async Task<TokenWithClaimsPrincipal> GetTokenWithClaimsPrincipal(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
            if (user == null)
            {
                return null;
            }

            var checkPasswordAsync = await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
            if (!checkPasswordAsync)
            {
                return null;
            }

            IEnumerable<Claim> claims = await AddDefaultClaims(user).ConfigureAwait(false);
            var tokenWithClaimsPrincipal = _tokenGenerator.GetAccessTokenWithClaimsPrincipal(user.UserName, claims);

            return tokenWithClaimsPrincipal;
        }

        public async Task<string> GetJwtToken(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
            if (user == null)
            {
                return null;
            }

            var checkPasswordAsync = await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
            if (!checkPasswordAsync)
            {
                return null;
            }

            IEnumerable<Claim> claims = await AddDefaultClaims(user).ConfigureAwait(false);
            var jwtResponse = _tokenGenerator.GetJwtToken(user.UserName, claims);

            return jwtResponse;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _userManager?.Dispose();
            }
        }

        private async Task<IEnumerable<Claim>> AddDefaultClaims(JobApplicationUser user)
        {
            var claims = new List<Claim> { new Claim(JwtClaimIdentifiers.Id, user.Id) };

            IList<string> roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            claims.AddRange(roles.Select(role => new Claim(JwtClaimIdentifiers.Role, role)));

            return claims;
        }
    }
}
