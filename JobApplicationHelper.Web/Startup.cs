using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationHelper.Web
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

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ApiOptions>(Configuration.GetSection("ApiOptions"));

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

            app.Run(async (context) => { await context.Response.WriteAsync("Hello World!"); });

            appLifetime.ApplicationStopped.Register(() => _aspNetScope.Dispose());
        }
    }
}
