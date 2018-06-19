using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

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
            var response = await Client.GetAsync(requestUri).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonConvert.DeserializeObject<string>(response.Content.ReadAsStringAsync().Result);
                return Ok(responseData);
            }

            return HandleFailureResponse(requestUri, response.StatusCode);
        }
    }
}
