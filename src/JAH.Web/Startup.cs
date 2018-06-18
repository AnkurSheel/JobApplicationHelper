using System;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using JAH.Logger;
using JAH.Logger.Middleware;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JAH.Web
{
    public class Startup
    {
        private readonly ILifetimeScope _webHostScope;

        private readonly IHostingEnvironment _hostingEnvironment;

        private ILifetimeScope _aspNetScope;

        public Startup(ILifetimeScope webHostScope, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Configuration = configuration;
            _webHostScope = webHostScope ?? throw new ArgumentNullException(nameof(webHostScope));
            _hostingEnvironment = hostingEnvironment;
        }

        private IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<UserHelper, UserHelper>();

            ConfigureLogger(services);

            services.AddMvc();
            _aspNetScope = _webHostScope.BeginLifetimeScope(builder => builder.Populate(services));

            return new AutofacServiceProvider(_aspNetScope);
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime, IHostingEnvironment env)
        {
            app.UseCustomExceptionHandler("JAH", "Web", "/Error");
            if (env.IsDevelopment())
            {
                TelemetryConfiguration.Active.DisableTelemetry = true;
            }

            app.UseStaticFiles();

            app.UseMvc();

            app.Run(async context => { await context.Response.WriteAsync("Hello World").ConfigureAwait(false); });
            appLifetime.ApplicationStopped.Register(() => _aspNetScope.Dispose());
        }

        protected virtual void ConfigureLogger(IServiceCollection services)
        {
            services.AddSingleton<IJahLogger, JahLogger>(s => new JahLogger(Configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
