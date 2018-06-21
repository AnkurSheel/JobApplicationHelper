using System.Threading.Tasks;

using JAH.Api.Filters;
using JAH.DomainModels;
using JAH.Logger.Attributes;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Api.Controllers
{
    [ValidateModel]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAccountManagerService _accountManagerService;

        public AuthController(IAccountManagerService accountManagerService)
        {
            _accountManagerService = accountManagerService;
        }

        [HttpGet("signedIn")]
        [AllowAnonymous]
        public IActionResult SignedIn()
        {
            return Ok(HttpContext.User.Identity.IsAuthenticated);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "Auth", "Api", "Login" })]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
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

        [HttpPost("logout")]
        [Authorize]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "Auth", "Api", "Logout" })]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync().ConfigureAwait(false);
            return Ok();
        }

        [HttpPost("token")]
        [AllowAnonymous]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "Auth", "Api", "GetToken" })]
        public async Task<IActionResult> GetToken([FromBody] LoginModel model)
        {
            var jwtResponse = await _accountManagerService.GetJwtToken(model).ConfigureAwait(false);
            if (string.IsNullOrEmpty(jwtResponse))
            {
                return BadRequest("Failed to Login");
            }

            return Ok(jwtResponse);
        }
    }
}
