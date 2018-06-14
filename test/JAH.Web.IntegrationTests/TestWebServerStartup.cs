using Autofac;

using JAH.Logger;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JAH.Web.IntegrationTests
{
    internal class TestWebServerStartup : Startup
    {
        public TestWebServerStartup(ILifetimeScope webHostScope, IHostingEnvironment env, IConfiguration configuration)
            : base(webHostScope, env, configuration)
        {
        }

        protected override void ConfigureLogger(IServiceCollection services)
        {
            services.AddSingleton<IJahLogger, FakeJahLogger>();
        }
    }
}
