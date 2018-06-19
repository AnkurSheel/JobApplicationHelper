using System;
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
    public class JobApplicationsController : BaseController
    {
        public JobApplicationsController(HttpClient client)
            : base(client)
        {
            ApiUri = new Uri(client.BaseAddress, "api/jobApplications/");
        }

        [HttpGet]
        public async Task<IActionResult> ListAllApplications()
        {
            var response = await Client.GetAsync(ApiUri).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseData = response.Content.ReadAsStringAsync().Result;
                IEnumerable<JobApplication> applications =
                    JsonConvert.DeserializeObject<IEnumerable<JobApplication>>(responseData) ?? new List<JobApplication>();
                return View(applications);
            }

            return HandleFailureResponse(ApiUri, response.StatusCode);
        }

        [HttpGet]
        [Route("{companyName}")]
        public async Task<IActionResult> ShowApplication(string companyName)
        {
            var requestUri = new Uri(ApiUri, $"{companyName}");
            var response = await Client.GetAsync(requestUri).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseData = response.Content.ReadAsStringAsync().Result;
                var application = JsonConvert.DeserializeObject<JobApplication>(responseData) ?? new JobApplication();
                return View(application);
            }

            return HandleFailureResponse(requestUri, response.StatusCode);
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
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(jobApplication);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await Client.PostAsync(ApiUri, stringContent).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListAllApplications");
                }

                return HandleFailureResponse(ApiUri, response.StatusCode);
            }

            return View(jobApplication);
        }

        [HttpPost]
        [Route("updateApplication")]
        public async Task<IActionResult> UpdateApplication(int id,
                                                           [Bind("Id, CompanyName, ApplicationDate, Status")]
                                                           JobApplication application)
        {
            var json = JsonConvert.SerializeObject(application);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var requestUri = new Uri(ApiUri, $"{application.CompanyName}");
            var response = await Client.PutAsync(requestUri, stringContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ListAllApplications");
            }

            return HandleFailureResponse(requestUri, response.StatusCode);
        }
    }
}
