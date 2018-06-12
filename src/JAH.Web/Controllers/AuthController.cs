using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using JAH.DomainModels;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace JAH.Web.Controllers
{
    [Route("[controller]")]
    public class AuthController : BaseController
    {
        public AuthController(HttpClient client)
            : base(client)
        {
            ApiUri = new Uri(client.BaseAddress, "api/auth/");
        }

        [HttpGet]
        [Route("")]
        [Route("~/")]
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            var responseMessage = await Client.GetAsync(new Uri(ApiUri, "signedIn")).ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var signedIn = JsonConvert.DeserializeObject<bool>(responseData);
                if (signedIn)
                {
                    return RedirectToAction("ListAllApplications", "JobApplications");
                }
            }

            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var json = JsonConvert.SerializeObject(model);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var responseMessage = await Client.PostAsync(new Uri(ApiUri, "login"), stringContent).ConfigureAwait(false);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("ListAllApplications", "JobApplications");
                    }

                    ModelState.AddModelError(string.Empty, responseMessage.Content.ReadAsStringAsync().Result);
                    ModelState.AddModelError(string.Empty, "UserName/Password Not found");
                }

                return View(model);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var responseMessage = await Client.PostAsync(new Uri(ApiUri, "logout"), null).ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login");
                }

                return new StatusCodeResult((int)responseMessage.StatusCode);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }
    }
}
