using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

namespace JAH.Web.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            return View();
        }
    }
}
