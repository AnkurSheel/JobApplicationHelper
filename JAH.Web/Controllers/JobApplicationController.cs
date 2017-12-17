using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
                    IEnumerable<JobApplication> applications =
                        JsonConvert.DeserializeObject<IEnumerable<JobApplication>>(responseData) ?? new List<JobApplication>();
                    return View(applications);
                }

                return new StatusCodeResult((int)responseMessage.StatusCode);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }

        [HttpGet]
        [Route("NewApplication")]
        public IActionResult NewApplication()
        {
            return View();
        }

        [HttpPost]
        [Route("")]
        [Route("NewApplication")]
        public async Task<IActionResult> NewApplication(JobApplication jobApplication)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string json = JsonConvert.SerializeObject(jobApplication);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage responseMessage = await _client.PostAsync($"api/jobApplication", stringContent);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("GetAsync");
                    }

                    return new StatusCodeResult((int)responseMessage.StatusCode);
                }

                return View(jobApplication);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }
    }
}
