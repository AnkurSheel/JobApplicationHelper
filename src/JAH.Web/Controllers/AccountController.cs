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
    public class AccountController : BaseController
    {
        public AccountController(HttpClient client)
            : base(client)
        {
            ApiUri = new Uri(client.BaseAddress, "api/account/");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(CredentialModel credentials)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string json = JsonConvert.SerializeObject(credentials);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage responseMessage = await Client.PostAsync(ApiUri, stringContent).ConfigureAwait(false);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    ModelState.AddModelError(string.Empty, responseMessage.Content.ReadAsStringAsync().Result);
                    ModelState.AddModelError(string.Empty, "Could not register user");
                }

                return View(credentials);
            }
            catch (HttpRequestException)
            {
                return new StatusCodeResult(501);
            }
        }
    }
}
