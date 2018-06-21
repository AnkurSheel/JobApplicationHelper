using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JAH.Api.Filters;
using JAH.DomainModels;
using JAH.Logger.Attributes;
using JAH.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace JAH.Api.Controllers
{
    [Route("api/[controller]")]
    [ValidateModel]
    public class JobApplicationsController : BaseController
    {
        private readonly IJobApplicationService _service;

        public JobApplicationsController(IJobApplicationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet("")]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "JobApplications", "Api", "GetApplications" })]
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
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "JobApplications", "Api", "GetApplication" })]
        public IActionResult Get(string companyName)
        {
            var jobApplication = _service.GetApplication(companyName);
            if (jobApplication == null)
            {
                return NotFound($"Company with Name \"{companyName}\" was not found");
            }

            return Ok(jobApplication);
        }

        [HttpPost]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "JobApplications", "Api", "CreateApplication" })]
        public async Task<IActionResult> Post([FromBody] JobApplication jobApplication)
        {
            var createdJobApplication = await _service.AddNewApplication(jobApplication).ConfigureAwait(false);
            return CreatedAtRoute("GetJobApplication", new { companyName = createdJobApplication.CompanyName }, createdJobApplication);
        }

        [HttpPut("{companyName}")]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "JobApplications", "Api", "UpdateApplication" })]
        public async Task<IActionResult> Put(string companyName, [FromBody] JobApplication jobApplication)
        {
            var oldApplication = await _service.UpdateApplication(companyName, jobApplication).ConfigureAwait(false);
            if (oldApplication == null)
            {
                return NotFound($"Company with Name \"{companyName}\" was not found");
            }

            return Ok(oldApplication);
        }
    }
}
