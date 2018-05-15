using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JAH.Api.Controllers
{
    public abstract class BaseController : Controller
    {
        public const string Urlhelper = "UrlHelper";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            context.HttpContext.Items[Urlhelper] = Url;
        }
    }
}
