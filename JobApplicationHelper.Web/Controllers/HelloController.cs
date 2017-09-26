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
        readonly HttpClient _client;
        private readonly string _uri;

        public HelloController(IOptions<ApiOptions> apiOptions)
        {
            _uri = $"{apiOptions.Value.BaseUri}hello";
            _client = new HttpClient {BaseAddress = new Uri(_uri)};
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
