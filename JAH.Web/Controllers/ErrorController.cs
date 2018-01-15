using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JAH.Web.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        [Route("{statusCode:int?}")]
        public IActionResult Index(int? statusCode)
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            ViewBag.StatusCode = statusCode;
            ViewBag.OriginalPath = feature?.OriginalPath;
            ViewBag.OriginalQueryString = feature?.OriginalQueryString;

            return View();
        }
    }
}
