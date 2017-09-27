using System;
using System.Threading.Tasks;
using JobApplicationHelper.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationHelper.Api.Controllers
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
        public async Task<IActionResult> List()
        {
            var jobApplications = await _service.ReadAllAsync();
            return Ok(jobApplications);
        }
    }
}