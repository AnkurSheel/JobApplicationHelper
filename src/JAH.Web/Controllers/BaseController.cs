using System;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc;

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
    }
}
