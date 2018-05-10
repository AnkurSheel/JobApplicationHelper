using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Api.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        [HttpGet]
        [Route("~/")]
        [AllowAnonymous]
        public IActionResult Greet()
        {
            return Ok("Hello user");
        }

        [HttpGet("{name}")]
        public IActionResult Greet(string name)
        {
            return Ok($"Hello {name}");
        }
    }
}
