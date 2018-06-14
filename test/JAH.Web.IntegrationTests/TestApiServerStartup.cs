using System;

using Autofac;

using JAH.Data;
using JAH.Logger;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace JAH.Web.IntegrationTests
{
    internal class TestApiServerStartup : Api.Startup
    {
        private static readonly LoggerFactory MyLoggerFactory =
            new LoggerFactory(new[] { new DebugLoggerProvider((_, level) => level >= LogLevel.Information) });

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
                                                                      .EnableSensitiveDataLogging()
                                                                      .UseLoggerFactory(MyLoggerFactory),
                                                           ServiceLifetime.Singleton);
        }

        protected override void ConfigureLogger(IServiceCollection services)
        {
            services.AddSingleton<IJahLogger, FakeJahLogger>();
        }
    }
}
