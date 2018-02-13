using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JAH.DomainModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JAH.Web.Controllers
{
    [Route("[controller]")]
    public class JobApplicationsController : Controller
    {
        private const string ApiUriBasePath = "api/jobApplications";

        private readonly HttpClient _client;

        public JobApplicationsController(HttpClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> ListAllApplications()
        {
            try
            {
                HttpResponseMessage responseMessage = await _client.GetAsync(ApiUriBasePath);
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
        [Route("{companyName}")]
        public async Task<IActionResult> ShowApplication(string companyName)
        {
            try
            {
                HttpResponseMessage responseMessage = await _client.GetAsync($"{ApiUriBasePath}/{companyName}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    string responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    JobApplication application = JsonConvert.DeserializeObject<JobApplication>(responseData) ?? new JobApplication();
                    return View(application);
                }

                return new StatusCodeResult((int)responseMessage.StatusCode);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }

        [HttpGet]
        [Route("addNewApplication")]
        public IActionResult AddNewApplication()
        {
            return View();
        }

        [HttpPost]
        [Route("addNewApplication")]
        public async Task<IActionResult> AddNewApplication(JobApplication jobApplication)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string json = JsonConvert.SerializeObject(jobApplication);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage responseMessage = await _client.PostAsync(ApiUriBasePath, stringContent);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("ListAllApplications");
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

        [HttpPost]
        [Route("updateApplication")]
        public async Task<IActionResult> UpdateApplication(int id,
                                                           [Bind("Id, CompanyName, ApplicationDate, Status")]
                                                           JobApplication application)
        {
            string json = JsonConvert.SerializeObject(application);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await _client.PutAsync($"{ApiUriBasePath}/{application.CompanyName}", stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("ListAllApplications");
            }

            return new StatusCodeResult((int)responseMessage.StatusCode);
        }
    }
}
