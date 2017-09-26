using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JobApplicationHelper.Web.Controllers
{
    [Route("api/hello")]
    public class HelloController : Controller
    {
        [HttpGet("{name}")]
        public IActionResult Greet(string name)
        {
            return Ok($"Hello {name}");
        }
    }
}
