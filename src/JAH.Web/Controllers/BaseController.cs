using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly HttpClient Client;

        public BaseController(HttpClient client)
        {
            Client = client;
        }
    }
}
