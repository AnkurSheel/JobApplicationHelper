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
            try
            {
                var requestUri = new Uri(ApiUri, $"{name}");
                var responseMessage = await Client.GetAsync(requestUri).ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;

                    return Ok(responseData);
                }

                return new StatusCodeResult((int)responseMessage.StatusCode);
            }
            catch (Exception)
            {
                return new StatusCodeResult(501);
            }
        }
    }
}
