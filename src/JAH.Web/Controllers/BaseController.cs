using System;
using System.Net;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc;

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

        protected IActionResult HandleFailureResponse(Uri requestUri, HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized();
            }

            if (statusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            var ex = new ApiCallException(requestUri, statusCode);
            throw ex;
        }
    }
}
