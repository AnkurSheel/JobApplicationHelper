using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JAH.DomainModels;
using JAH.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace JAH.Api.Controllers
{
    [Route("api/[controller]")]
    public class JobApplicationsController : Controller
    {
        private readonly ILogger<JobApplicationsController> _logger;
        private readonly IJobApplicationService _service;

        public JobApplicationsController(IJobApplicationService service, ILogger<JobApplicationsController> logger)
        {
            _logger = logger;
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            IEnumerable<JobApplication> jobApplications = await _service.GetAllApplications();
            if (jobApplications.Any())
            {
                return Ok(jobApplications);
            }

            return NoContent();
        }

        [HttpGet("{companyName}", Name = "JobApplicationsGet")]
        public async Task<IActionResult> Get(string companyName)
        {
            try
            {
                JobApplication jobApplication = await _service.GetApplication(companyName);
                if (jobApplication == null)
                {
                    return NotFound($"Company with Name \"{companyName}\" was not found");
                }

                return Ok(jobApplication);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.JobApplications, e, $"Exception when trying to get application for application with Name \"{companyName}\"");
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JobApplication jobApplication)
        {
            try
            {
                JobApplication createdJobApplication = await _service.AddNewApplication(jobApplication);
                return CreatedAtRoute("JobApplicationsGet", new { companyName = createdJobApplication.CompanyName }, createdJobApplication);
            }
            catch (ArgumentException)
            {
                var modelState = new ModelStateDictionary();
                modelState.AddModelError("Duplicate Name", $"Name {jobApplication.CompanyName} already exists.");
                return BadRequest(modelState);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.JobApplications, e, $"Exception when trying to create application for application with Name \"{jobApplication.CompanyName}\"");
            }

            return BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] JobApplication jobApplication)
        {
            try
            {
                await _service.UpdateApplication(jobApplication);

                return Ok();
            }
            catch (DBConcurrencyException e)
            {
                var modelState = new ModelStateDictionary();
                modelState.AddModelError("Application Does Not Exist", $"Application for {jobApplication.CompanyName} does not exist.");
                return BadRequest(modelState);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
