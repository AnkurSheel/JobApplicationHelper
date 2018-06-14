using System;
using System.Net.Http;
using System.Threading.Tasks;

using JAH.Logger;

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
        [TypeFilter(typeof(TrackUsageAttribute), Arguments = new object[] { "Hello", "Web", "Greet" })]
        public async Task<IActionResult> Greet(string name)
        {
            try
            {
                var requestUri = new Uri(ApiUri, $"{name}");
                HttpResponseMessage responseMessage = await Client.GetAsync(requestUri).ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    string responseData = responseMessage.Content.ReadAsStringAsync().Result;

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
