using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JAH.Api
{
    public class Startup
    {
        private readonly ILifetimeScope _webHostScope;
        private readonly IHostingEnvironment _env;
        private ILifetimeScope _aspNetScope;

        public Startup(ILifetimeScope webHostScope, IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            _webHostScope = webHostScope ?? throw new ArgumentNullException(nameof(webHostScope));
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper();

            services.AddMvc(opt =>
            {
                if (!_env.IsProduction())
                {
                    opt.SslPort = 44324;
                }

                opt.Filters.Add(new RequireHttpsAttribute());
            });

            _aspNetScope = _webHostScope.BeginLifetimeScope(builder => builder.Populate(services));

            return new AutofacServiceProvider(_aspNetScope);
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                TelemetryConfiguration.Active.DisableTelemetry = true;
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            appLifetime.ApplicationStopped.Register(() => _aspNetScope.Dispose());
        }
    }
}
