using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JobApplicationHelper.Web.Controllers
{
    [Route("hello")]
    public class HelloController : Controller
    {
        readonly HttpClient _client;
        private readonly string _uri;

        public HelloController()
        {
            _uri = "http://localhost:4064/api/hello";
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
