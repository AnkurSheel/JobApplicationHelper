using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JobApplicationHelper.Web.Controllers
{
    [Route("hello")]
    public class HelloController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _uri;

        public HelloController(HttpClient client)
        {
            _client = client;
            _uri = $"{client.BaseAddress}hello";
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Greet(string name)
        {
            HttpResponseMessage responseMessage = await _client.GetAsync(_uri + $"/{name}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;

                return Ok(responseData);
            }

            return Ok("error");
        }
    }
}
