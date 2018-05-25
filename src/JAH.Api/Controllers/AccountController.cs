﻿using System;
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
    public class AccountController : Controller
    {
        private readonly UserManager<JobApplicationUser> _userManager;

        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<JobApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
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
    }
}
