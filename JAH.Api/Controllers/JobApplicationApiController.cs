using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAH.DomainModels;
using JAH.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JAH.Api.Controllers
{
    [Route("api/JobApplication")]
    public class JobApplicationApiController : Controller
    {
        private readonly IJobApplicationService _service;

        public JobApplicationApiController(IJobApplicationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            IEnumerable<JobApplication> jobApplications = await _service.ReadAllAsync();
            if (jobApplications.Any())
            {
                return Ok(jobApplications);
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] JobApplication jobApplication)
        {
            try
            {
                await _service.AddNewApplication(jobApplication);
                return CreatedAtAction("GetAsync", new { id = jobApplication.CompanyName }, jobApplication);
            }
            catch (ArgumentException)
            {
                var modelState = new ModelStateDictionary();
                modelState.AddModelError("Duplicate Name", $"Name {jobApplication.CompanyName} already exists.");
                return BadRequest(modelState);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            ;
        }
    }
}
