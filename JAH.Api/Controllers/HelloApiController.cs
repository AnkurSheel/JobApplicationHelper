using Microsoft.AspNetCore.Mvc;

namespace JAH.Api.Controllers
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
