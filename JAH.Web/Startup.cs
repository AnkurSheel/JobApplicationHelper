﻿using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
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
        private ILifetimeScope _aspNetScope;

        public Startup(ILifetimeScope webHostScope, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Configuration = configuration;
            _webHostScope = webHostScope ?? throw new ArgumentNullException(nameof(webHostScope));
        }

        private IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            _aspNetScope = _webHostScope.BeginLifetimeScope(builder => builder.Populate(services));

            return new AutofacServiceProvider(_aspNetScope);
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime, IHostingEnvironment env)
        {
            app.UseMvc();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.Run(async (context) => { await context.Response.WriteAsync("Hello World!"); });

            appLifetime.ApplicationStopped.Register(() => _aspNetScope.Dispose());
        }
    }
}