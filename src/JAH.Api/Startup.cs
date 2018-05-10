using System;
using System.Threading.Tasks;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using AutoMapper;

using JAH.Data;
using JAH.Data.Entities;
using JAH.Services.Interfaces;
using JAH.Services.Services;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace JAH.Api
{
    public class Startup
    {
        private static readonly LoggerFactory MyLoggerFactory =
            new LoggerFactory(new[] { new DebugLoggerProvider((_, level) => level >= LogLevel.Information) });

        private readonly IHostingEnvironment _env;

        private readonly ILifetimeScope _webHostScope;

        private ILifetimeScope _aspNetScope;

        public Startup(ILifetimeScope webHostScope, IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            _webHostScope = webHostScope ?? throw new ArgumentNullException(nameof(webHostScope));
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                TelemetryConfiguration.Active.DisableTelemetry = true;
                app.UseDeveloperExceptionPage();
            }

            ConfigureAdditionalMiddleware(app);

            app.UseAuthentication();
            app.UseMvc();

            appLifetime.ApplicationStopped.Register(() => _aspNetScope.Dispose());
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserResolverService, UserResolverService>();
            services.AddTransient<DbSeeder, DbSeeder>();

            services.AddAutoMapper();

            ConfigureDatabase(services);

            services.AddIdentity<JobApplicationUser, IdentityRole>(options =>
                    {
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequiredLength = 5;
                        options.Password.RequiredUniqueChars = 0;
                        options.Password.RequireDigit = false;
                    })
                    .AddEntityFrameworkStores<JobApplicationDbContext>();
            services.ConfigureApplicationCookie(options => options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                    {
                        ctx.Response.StatusCode = 401;
                    }

                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                    {
                        ctx.Response.StatusCode = 403;
                    }

                    return Task.CompletedTask;
                }
            });
            services.AddMvc(opt =>
            {
                if (!_env.IsProduction())
                {
                    opt.SslPort = 44324;
                }

                if (!_env.IsEnvironment("Testing"))
                {
                    opt.Filters.Add(new RequireHttpsAttribute());
                }

                AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                opt.Filters.Add(new AuthorizeFilter(policy));
            });

            _aspNetScope = _webHostScope.BeginLifetimeScope(builder => builder.Populate(services));

            return new AutofacServiceProvider(_aspNetScope);
        }

        protected virtual void ConfigureAdditionalMiddleware(IApplicationBuilder app)
        {
        }

        protected virtual void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<JobApplicationDbContext>(options => options
                                                                      .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                                                                      .UseLoggerFactory(MyLoggerFactory));
        }
    }
}
