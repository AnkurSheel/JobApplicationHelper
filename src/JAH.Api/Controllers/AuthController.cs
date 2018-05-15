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
    public class AuthController : Controller
    {
        private readonly SignInManager<JobApplicationUser> _signInManager;

        private readonly UserManager<JobApplicationUser> _userManager;

        private readonly ILogger<AuthController> _logger;

        /// <inheritdoc />
        public AuthController(SignInManager<JobApplicationUser> signInManager,
                              UserManager<JobApplicationUser> userManager,
                              ILogger<AuthController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("api/auth/signedIn")]
        [AllowAnonymous]
        public IActionResult SignedIn()
        {
            try
            {
                bool result = _signInManager.IsSignedIn(User);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to register");
                return BadRequest(e);
            }
        }

        [HttpPost("api/auth/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CredentialModel model)
        {
            try
            {
                var jobApplicationUser = new JobApplicationUser { UserName = model.UserName };
                IdentityResult result = await _userManager.CreateAsync(jobApplicationUser, model.Password).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(3, "User created a new account with password.");
                    return Ok();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to register");
                return BadRequest(e);
            }

            return BadRequest("Failed to Register");
        }

        [HttpPost("api/auth/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
                Microsoft.AspNetCore.Identity.SignInResult result =
                    await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false).ConfigureAwait(false);
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

        [HttpPost("api/auth/logout")]
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
