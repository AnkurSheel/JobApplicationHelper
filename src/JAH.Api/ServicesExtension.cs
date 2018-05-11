using System;
using System.Threading.Tasks;

using JAH.Data;
using JAH.Data.Entities;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace JAH.Api
{
    public static class ServicesExtension
    {
        public static void AddSecurity(this IServiceCollection services)
        {
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

            services.ConfigureApplicationCookie(options => options.Events = GetCookieAuthenticationEvents());
        }

        public static void AddCustomizedMVC(this IServiceCollection services, IHostingEnvironment hostingEnvironment)
        {
            services.AddMvc(opt =>
            {
                if (!hostingEnvironment.IsProduction())
                {
                    opt.SslPort = 44324;
                }

                if (!hostingEnvironment.IsEnvironment("Testing"))
                {
                    opt.Filters.Add(new RequireHttpsAttribute());
                }

                AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                opt.Filters.Add(new AuthorizeFilter(policy));
            });
        }

        private static CookieAuthenticationEvents GetCookieAuthenticationEvents()
        {
            return new CookieAuthenticationEvents
                   {
                       OnRedirectToLogin = ctx =>
                       {
                           if (ctx.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCultureIgnoreCase)
                               && ctx.Response.StatusCode == 200)
                           {
                               ctx.Response.StatusCode = 401;
                           }

                           return Task.CompletedTask;
                       },
                       OnRedirectToAccessDenied = ctx =>
                       {
                           if (ctx.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCultureIgnoreCase)
                               && ctx.Response.StatusCode == 200)
                           {
                               ctx.Response.StatusCode = 403;
                           }

                           return Task.CompletedTask;
                       }
                   };
        }
    }
}
