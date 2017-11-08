using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Web.Controllers
{
    [Route("hello")]
    public class HelloController : Controller
    {
        private readonly HttpClient _client;

        public HelloController(HttpClient client)
        {
            _client = client;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Greet(string name)
        {
            HttpResponseMessage responseMessage = await _client.GetAsync($"api/hello/{name}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;

                return Ok(responseData);
            }

            return Ok("error");
        }
    }
}
