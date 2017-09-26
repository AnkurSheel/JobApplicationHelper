using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationHelper.Api.Controllers
{
    [Route("api/hello")]
    public class HelloApiController : Controller
    {
        [HttpGet("{name}")]
        public IActionResult Greet(string name)
        {
            return Ok($"Hello {name}");
        }
    }
}
