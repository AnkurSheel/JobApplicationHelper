﻿using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using AutoMapper;

using JAH.Data;
using JAH.Helper;
using JAH.Services.Interfaces;
using JAH.Services.Services;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

using Swashbuckle.AspNetCore.Swagger;

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
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }

            ConfigureAdditionalMiddleware(app);

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JAH API v1"));

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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "JAH Api", Version = "v1" });
                var apiKeyScheme = new ApiKeyScheme
                {
                    In = "header",
                    Description = "Please enter JWT with Bearer into field",
                    Name = "Authorization",
                    Type = "apiKey"
                };
                c.AddSecurityDefinition("Bearer", apiKeyScheme);

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "Bearer", Enumerable.Empty<string>() } });
                c.DescribeAllEnumsAsStrings();
            });
            ConfigureDatabase(services);

            services.AddSecurity(_env, Configuration.GetSection(nameof(TokenOptions)));
            services.AddCustomizedMvc(_env);

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