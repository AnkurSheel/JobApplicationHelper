using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using JAH.Data;
using JAH.Data.Entities;
using JAH.Helper;
using JAH.Helper.Constants;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace JAH.Api
{
    public static class ServicesExtension
    {
        public static void AddJwtSecurity(this IServiceCollection services, IHostingEnvironment env, IConfigurationSection jwtAppSettingOptions)
        {
            AddSecurityCommon(services);
            services.AddDataProtection(options => options.ApplicationDiscriminator = env.ApplicationName).SetApplicationName(env.ApplicationName);

            services.AddScoped<IDataSerializer<AuthenticationTicket>, TicketSerializer>();

            var issuer = jwtAppSettingOptions[nameof(Helper.TokenOptions.Issuer)];
            var audience = jwtAppSettingOptions[nameof(Helper.TokenOptions.Audience)];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAppSettingOptions["SigningKey"]));
            var tokenExpiryInMinutes =
                Convert.ToInt32(jwtAppSettingOptions[nameof(Helper.TokenOptions.TokenExpiryInMinutes)], CultureInfo.CurrentCulture);

            var tokenValidationParameters = new TokenValidationParameters
                                            {
                                                RequireExpirationTime = true,
                                                RequireSignedTokens = true,
                                                ValidateIssuerSigningKey = true,
                                                IssuerSigningKey = key,
                                                ValidateIssuer = true,
                                                ValidIssuer = issuer,
                                                ValidateAudience = true,
                                                ValidAudience = audience,
                                                ValidateLifetime = true,
                                                ClockSkew = TimeSpan.Zero
                                            };

            services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddCookie(options =>
                    {
                        options.Cookie.Expiration = TimeSpan.FromMinutes(tokenExpiryInMinutes);
                        options.TicketDataFormat = new CustomJwtDataFormat(tokenValidationParameters,
                                                                           services.BuildServiceProvider()
                                                                                   .GetService<IDataSerializer<AuthenticationTicket>>(),
                                                                           services.BuildServiceProvider()
                                                                                   .GetDataProtector(new[] { $"{env.ApplicationName}-Auth1" }));
                    })
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.ClaimsIssuer = issuer;
                        options.TokenValidationParameters = tokenValidationParameters;
                        options.SaveToken = true;
                    });

            // api user claim policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireClaim(JwtClaimIdentifiers.Role, JwtClaims.Admin));
            });

            services.AddScoped<ITokenGenerator, TokenGenerator>(serviceProvider =>
                                                                    new TokenGenerator(tokenValidationParameters
                                                                                           .ToTokenOptions(tokenExpiryInMinutes)));
        }

        public static void AddCookieSecurity(this IServiceCollection services)
        {
            AddSecurityCommon(services);

            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                    })
                    .AddCookie(IdentityConstants.ApplicationScheme, o => { o.LoginPath = new PathString("/Account/Login"); })
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
        }

        public static void AddCustomizedMvc(this IServiceCollection services, IHostingEnvironment hostingEnvironment)
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

        private static void AddSecurityCommon(IServiceCollection services)
        {
            services.ConfigureApplicationCookie(cfg => cfg.Events = GetCookieAuthenticationEvents());

            ////services.TryAddScoped<IRoleValidator<IdentityRole>, RoleValidator<IdentityRole>>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<JobApplicationUser>>();
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
