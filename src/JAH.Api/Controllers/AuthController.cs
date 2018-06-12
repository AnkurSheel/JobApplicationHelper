using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using JAH.Api.Filters;
using JAH.Data.Entities;
using JAH.DomainModels;
using JAH.Helper;
using JAH.Helper.Constants;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JAH.Api.Controllers
{
    [ValidateModel]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<JobApplicationUser> _userManager;

        private readonly ILogger<AuthController> _logger;

        private readonly ITokenGenerator _tokenGenerator;

        public AuthController(UserManager<JobApplicationUser> userManager, ITokenGenerator tokenGenerator, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _tokenGenerator = tokenGenerator;
        }

        [HttpGet("signedIn")]
        [AllowAnonymous]
        public IActionResult SignedIn()
        {
            try
            {
                var result = HttpContext.User.Identity.IsAuthenticated;
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to register");
                return BadRequest(e);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                if (user != null)
                {
                    var checkPasswordAsync = await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
                    if (checkPasswordAsync)
                    {
                        IEnumerable<Claim> claims = await AddDefaultClaims(user).ConfigureAwait(false);
                        var tokenWithClaimsPrincipal = _tokenGenerator.GenerateAccessTokenWithClaimsPrincipal(user.UserName, claims);

                        await HttpContext.SignInAsync(tokenWithClaimsPrincipal.ClaimsPrincipal, tokenWithClaimsPrincipal.AuthenticationProperties)
                                         .ConfigureAwait(false);

                        return Ok(tokenWithClaimsPrincipal.JwtResponse);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to login");
                return BadRequest(e);
            }

            return BadRequest("Failed to Login");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync().ConfigureAwait(false);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to logout");
                return BadRequest(e);
            }
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetToken([FromBody] LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Email).ConfigureAwait(false);
                if (user != null)
                {
                    var checkPasswordAsync = await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
                    if (checkPasswordAsync)
                    {
                        IEnumerable<Claim> claims = await AddDefaultClaims(user).ConfigureAwait(false);
                        var jwtResponse = _tokenGenerator.GetJwtToken(user.UserName, claims);

                        return Ok(jwtResponse);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to login");
                return BadRequest(e);
            }

            return BadRequest("Failed to Login");
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
