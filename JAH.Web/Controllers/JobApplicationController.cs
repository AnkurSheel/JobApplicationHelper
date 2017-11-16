using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Web.Controllers
{
    [Route("jobApplication")]
    public class JobApplicationController : Controller
    {
        private readonly HttpClient _client;

        public JobApplicationController(HttpClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                HttpResponseMessage responseMessage = await _client.GetAsync($"api/jobApplication");
                if (responseMessage.IsSuccessStatusCode)
                {
                    string responseData = responseMessage.Content.ReadAsStringAsync().Result;

                    return Ok(responseData);
                }

                return new StatusCodeResult((int) responseMessage.StatusCode);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }
    }
}