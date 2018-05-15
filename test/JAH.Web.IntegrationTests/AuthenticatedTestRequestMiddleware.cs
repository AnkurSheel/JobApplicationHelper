using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace JAH.Web.IntegrationTests
{
    public class AuthenticatedTestRequestMiddleware
    {
        public const string TestingHeader = "X-Integration-Testing";

        public const string TestingHeaderValue = "abcde-12345";

        private const string TestingCookieAuthentication = "TestCookieAuthentication";

        private readonly RequestDelegate _next;

        public AuthenticatedTestRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains(TestingHeader) && context.Request.Headers[TestingHeader].First().Equals(TestingHeaderValue))
            {
                if (context.Request.Headers.Keys.Contains("my-name"))
                {
                    string name = context.Request.Headers["my-name"].First();
                    string id = context.Request.Headers.Keys.Contains("my-id") ? context.Request.Headers["my-id"].First() : string.Empty;
                    var claimsIdentity =
                        new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Name, name), new Claim(ClaimTypes.NameIdentifier, id) },
                                           TestingCookieAuthentication);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    context.User = claimsPrincipal;
                }
            }

            await _next(context).ConfigureAwait(true);
        }
    }
}
