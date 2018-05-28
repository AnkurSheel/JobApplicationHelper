using System;
using System.Threading.Tasks;

using JAH.Data;
using JAH.Data.Entities;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JAH.Api
{
    public static class ServicesExtension
    {
        public static void AddSecurity(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                    })
                    .AddCookie(IdentityConstants.ApplicationScheme,
                               o =>
                               {
                                   o.LoginPath = new PathString("/Account/Login");
                                   o.Events = GetCookieAuthenticationEvents();
                               })
                    .AddCookie(IdentityConstants.ExternalScheme,
                               o =>
                               {
                                   o.Cookie.Name = IdentityConstants.ExternalScheme;
                                   o.ExpireTimeSpan = TimeSpan.FromMinutes(5.0);
                               })
                    .AddCookie(IdentityConstants.TwoFactorUserIdScheme,
                               o =>
                               {
                                   o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
                                   o.ExpireTimeSpan = TimeSpan.FromMinutes(5.0);
                               });
            ////services.TryAddScoped<IRoleValidator<IdentityRole>, RoleValidator<IdentityRole>>();
            ////services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<JobApplicationUser>>();
            services.TryAddScoped<SignInManager<JobApplicationUser>, SignInManager<JobApplicationUser>>();
            services.TryAddScoped<RoleManager<IdentityRole>, AspNetRoleManager<IdentityRole>>();

            var builder = services.AddIdentityCore<JobApplicationUser>(options =>
            {
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireDigit = false;
            });
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddEntityFrameworkStores<JobApplicationDbContext>().AddDefaultTokenProviders();
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

                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                opt.Filters.Add(new AuthorizeFilter(policy));
            });
        }

        private static CookieAuthenticationEvents GetCookieAuthenticationEvents()
        {
            return new CookieAuthenticationEvents
                   {
                       OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync,
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
