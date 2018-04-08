using System;

using Autofac;

using JAH.Data;
using JAH.Data.Entities;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JAH.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (var container = IOCBuilder.Build())
                {
                    var host = BuildWebHost(container);
                    using (var scope = host.Services.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        try
                        {
                            SeedDatabase(services);
                        }
                        catch (Exception ex)
                        {
                            var logger = services.GetRequiredService<ILogger<Program>>();
                            logger.LogError(ex, "An error occurred while seeding the database.");
                        }
                    }

                    host.Run();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static IWebHost BuildWebHost(ILifetimeScope scope)
        {
            using (var webHostScope = scope.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
            {
                return WebHost.CreateDefaultBuilder().
                    UseStartup<Startup>().
                    ConfigureServices(services => services.AddTransient(provider =>
                        {
                            var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                            var config = provider.GetRequiredService<IConfiguration>();
                            var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Startup>>();
                            return factory(hostingEnv, config);
                        })).
                    Build();
            }
        }

        private static void SeedDatabase(IServiceProvider services)
        {
            try
            {
                var context = services.GetRequiredService<JobApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<JobApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var dbInitializerLogger = services.GetRequiredService<ILogger<DbInitializer>>();
                DbInitializer.Initialize(context, userManager, roleManager, dbInitializerLogger).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}