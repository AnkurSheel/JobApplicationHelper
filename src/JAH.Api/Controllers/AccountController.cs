using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JAH.Api.Filters;
using JAH.DomainModels;
using JAH.Logger.Attributes;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JAH.Api.Controllers
{
    [ValidateModel]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountManagerService _accountManagerService;

        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountManagerService accountManagerService, ILogger<AccountController> logger)
        {
            _accountManagerService = accountManagerService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "Account", "Api", "Register" })]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var result = await _accountManagerService.RegisterUser(model).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    _logger.LogInformation(3, "User created a new account with password.");
                    return Ok();
                }

                List<string> errors = result.Errors.Select(error => error.Description).ToList();

                return BadRequest(errors);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.Auth, e, $"Exception when trying to register");
                return BadRequest(e);
            }
        }
    }
}
