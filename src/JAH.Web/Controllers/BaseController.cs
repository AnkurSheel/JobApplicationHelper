using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace JAH.Web.Controllers
{
    public class BaseController : Controller
    {
        public BaseController(HttpClient client)
        {
            Client = client;
        }

        protected Uri ApiUri { get; set; }

        protected HttpClient Client { get; }

        protected async Task<IActionResult> Execute<T>(Uri requestUri)
        {
            var response = await Client.GetAsync(requestUri).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                return Ok(responseData);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            var ex = new ApiCallException(requestUri, response.StatusCode);
            throw ex;
        }
    }
}
