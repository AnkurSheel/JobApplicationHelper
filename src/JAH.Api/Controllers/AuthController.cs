using System;
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
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName).ConfigureAwait(false);
                if (user != null)
                {
                    var checkPasswordAsync = await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
                    if (checkPasswordAsync)
                    {
                        var tokenWithClaimsPrincipal = _tokenGenerator.GenerateAccessTokenWithClaimsPrincipal(user.UserName, AddDefaultClaims(user));

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
        public async Task<IActionResult> GetToken([FromBody] CredentialModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName).ConfigureAwait(false);
                if (user != null)
                {
                    var checkPasswordAsync = await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false);
                    if (checkPasswordAsync)
                    {
                        var jwtResponse = _tokenGenerator.GetJwtToken(user.UserName, AddDefaultClaims(user));

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

        private static Claim[] AddDefaultClaims(JobApplicationUser user)
        {
            return new[] { new Claim(JwtClaimIdentifiers.Id, user.Id), new Claim(JwtClaimIdentifiers.Role, JwtClaims.Admin) };
        }
    }
}
