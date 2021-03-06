using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JAH.Api.Filters;
using JAH.DomainModels;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JAH.Api.Controllers
{
    [Route("api/[controller]")]
    [ValidateModel]
    public class JobApplicationsController : BaseController
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
            IEnumerable<JobApplication> jobApplications = await _service.GetAllApplications().ConfigureAwait(false);
            if (jobApplications.Any())
            {
                return Ok(jobApplications);
            }

            return NoContent();
        }

        [HttpGet("{companyName}", Name = "GetJobApplication")]
        public IActionResult Get(string companyName)
        {
            try
            {
                JobApplication jobApplication = _service.GetApplication(companyName);
                if (jobApplication == null)
                {
                    return NotFound($"Company with Name \"{companyName}\" was not found");
                }

                return Ok(jobApplication);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.JobApplications,
                                 e,
                                 $"Exception when trying to get application for application with Name \"{companyName}\"");
                return BadRequest(e);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JobApplication jobApplication)
        {
            try
            {
                JobApplication createdJobApplication = await _service.AddNewApplication(jobApplication).ConfigureAwait(false);
                return CreatedAtRoute("GetJobApplication", new { companyName = createdJobApplication.CompanyName }, createdJobApplication);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.JobApplications,
                                 e,
                                 $"Exception when trying to create application for application with Name \"{jobApplication.CompanyName}\"");
                return BadRequest(e);
            }
        }

        [HttpPut("{companyName}")]
        public async Task<IActionResult> Put(string companyName, [FromBody] JobApplication jobApplication)
        {
            try
            {
                JobApplication oldApplication = await _service.UpdateApplication(companyName, jobApplication).ConfigureAwait(false);
                if (oldApplication == null)
                {
                    return NotFound($"Company with Name \"{companyName}\" was not found");
                }

                return Ok(oldApplication);
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.JobApplications,
                                 e,
                                 $"Exception when trying to create application for application with Name \"{jobApplication.CompanyName}\"");
                return BadRequest(e);
            }
        }
    }
}
