using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using JAH.DomainModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                HttpResponseMessage responseMessage = await _client.GetAsync($"api/jobApplication");
                if (responseMessage.IsSuccessStatusCode)
                {
                    string responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var applications = JsonConvert.DeserializeObject<IEnumerable<JobApplication>>(responseData) ?? new List<JobApplication>();
                    return View(applications);
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
