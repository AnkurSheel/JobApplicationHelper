using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationHelper.Api.Controllers
{
    public class JobApplicationApiController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> List()
        {
            throw new NotImplementedException();
        }
    }
}