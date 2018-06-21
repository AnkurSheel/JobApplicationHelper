using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JAH.Api.Filters;
using JAH.DomainModels;
using JAH.Logger.Attributes;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Api.Controllers
{
    [ValidateModel]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountManagerService _accountManagerService;

        public AccountController(IAccountManagerService accountManagerService)
        {
            _accountManagerService = accountManagerService;
        }

        [HttpPost]
        [AllowAnonymous]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "Account", "Api", "Register" })]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await _accountManagerService.RegisterUser(model).ConfigureAwait(false);

            if (result.Succeeded)
            {
                return Ok();
            }

            List<string> errors = result.Errors.Select(error => error.Description).ToList();

            return BadRequest(errors);
        }
    }
}
