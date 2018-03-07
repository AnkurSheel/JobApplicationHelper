using Microsoft.AspNetCore.Mvc;

namespace JAH.Api.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        [HttpGet("{name}")]
        public IActionResult Greet(string name)
        {
            return Ok($"Hello {name}");
        }
    }
}
