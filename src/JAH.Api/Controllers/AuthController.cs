using System;
using System.Threading.Tasks;

using JAH.Api.Filters;
using JAH.Data.Entities;
using JAH.DomainModels;

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
        private readonly SignInManager<JobApplicationUser> _signInManager;

        private readonly ILogger<AuthController> _logger;

        public AuthController(SignInManager<JobApplicationUser> signInManager, ILogger<AuthController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet("signedIn")]
        [AllowAnonymous]
        public IActionResult SignedIn()
        {
            try
            {
                var result = _signInManager.IsSignedIn(User);
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
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return Ok();
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
                await _signInManager.SignOutAsync().ConfigureAwait(false);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to logout");
                return BadRequest(e);
            }
        }
    }
}
