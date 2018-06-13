using System;
using System.Threading.Tasks;

using JAH.Api.Filters;
using JAH.DomainModels;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JAH.Api.Controllers
{
    [ValidateModel]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAccountManagerService _accountManagerService;

        private readonly ILogger<AuthController> _logger;

        public AuthController(IAccountManagerService accountManagerService, ILogger<AuthController> logger)
        {
            _accountManagerService = accountManagerService;
            _logger = logger;
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
                var tokenWithClaimsPrincipal = await _accountManagerService.GetTokenWithClaimsPrincipal(model).ConfigureAwait(false);
                if (tokenWithClaimsPrincipal == null)
                {
                    return BadRequest("Failed to Login");
                }

                await HttpContext.SignInAsync(tokenWithClaimsPrincipal.ClaimsPrincipal, tokenWithClaimsPrincipal.AuthenticationProperties)
                                 .ConfigureAwait(false);

                return Ok(tokenWithClaimsPrincipal.JwtResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to login");
                return BadRequest(e);
            }
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
                var jwtResponse = await _accountManagerService.GetJwtToken(model).ConfigureAwait(false);
                if (string.IsNullOrEmpty(jwtResponse))
                {
                    return BadRequest("Failed to Login");
                }

                return Ok(jwtResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to login");
                return BadRequest(e);
            }
        }
    }
}
