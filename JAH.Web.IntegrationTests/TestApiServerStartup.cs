using System;

using Autofac;

using JAH.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        /// <inheritdoc />
        protected override void ConfigureDatabase(IServiceCollection services)
        {
            services.AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<JobApplicationDbContext>(options => options
                                                                      .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                                      .EnableSensitiveDataLogging(), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        }
    }
}
