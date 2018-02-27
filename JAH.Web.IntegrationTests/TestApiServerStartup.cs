using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace JAH.Web.IntegrationTests
{
    internal class TestApiServerStartup : Api.Startup
    {
        /// <inheritdoc />
        public TestApiServerStartup(ILifetimeScope webHostScope, IHostingEnvironment env, IConfiguration configuration)
            : base(webHostScope, env, configuration)
        {
        }

        /// <inheritdoc />
        protected override void ConfigureAdditionalMiddleware(IApplicationBuilder app)
        {
            app.UseMiddleware<AuthenticatedTestRequestMiddleware>();
        }
    }
}
