using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JAH.DomainModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JAH.Web.Controllers
{
    [Route("[controller]")]
    public class AccountController : BaseController
    {
        private const string ApiUriBasePath = "api/auth";

        /// <inheritdoc />
        public AccountController(HttpClient client)
            : base(client)
        {
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(CredentialModel credentials)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string json = JsonConvert.SerializeObject(credentials);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage responseMessage = await Client.PostAsync($"{ApiUriBasePath}/register", stringContent);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Login", null);
                    }

                    ModelState.AddModelError("", responseMessage.Content.ReadAsStringAsync().Result);
                    ModelState.AddModelError("", "Could not register user");
                }

                return View(credentials);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }

        [HttpGet("")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("")]
        [Route("Login")]
        public async Task<IActionResult> Login(CredentialModel credentials)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string json = JsonConvert.SerializeObject(credentials);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage responseMessage = await Client.PostAsync($"{ApiUriBasePath}/login", stringContent);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("ListAllApplications", "JobApplications");
                    }

                    ModelState.AddModelError("", responseMessage.Content.ReadAsStringAsync().Result);
                    ModelState.AddModelError("", "UserName/Password Not found");
                }

                return View(credentials);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                HttpResponseMessage responseMessage = await Client.GetAsync($"{ApiUriBasePath}/logout");
                if (responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListAllApplications", "JobApplications");
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
