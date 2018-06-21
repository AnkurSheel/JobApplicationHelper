using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using JAH.Logger;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace JAH.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected BaseController(HttpClient client)
        {
            Client = client;
        }

        protected Uri ApiUri { get; set; }

        protected HttpClient Client { get; }

        protected async Task<IActionResult> HandleFailureResponse(Uri requestUri, HttpStatusCode statusCode, HttpContent content)
        {
            if (statusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized();
            }

            if (statusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            var responseData = await content.ReadAsStringAsync().ConfigureAwait(false);
            var error = JsonConvert.DeserializeObject<CustomErrorResponse>(responseData);
            var ex = new ApiCallException(requestUri, statusCode, error);
            throw ex;
        }
    }
}
