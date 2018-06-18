using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace JAH.Web.Controllers
{
    [Route("[controller]")]
    public class HelloController : BaseController
    {
        public HelloController(HttpClient client)
            : base(client)
        {
            ApiUri = new Uri(client.BaseAddress, "api/hello/");
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Greet(string name)
        {
            var requestUri = new Uri(ApiUri, $"{name}");
            var result = await Execute<string>(requestUri).ConfigureAwait(false);
            return result;
        }
    }
}
