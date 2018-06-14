using JAH.Logger;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Api.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : BaseController
    {
        [HttpGet]
        [Route("~/")]
        [AllowAnonymous]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "Hello", "Api", "Greet" })]
        public IActionResult Greet()
        {
            return Ok("Hello user");
        }

        [HttpGet("{name}")]
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "Hello", "Api", "Greet" })]
        public IActionResult Greet(string name)
        {
            return Ok($"Hello {name}");
        }
    }
}
