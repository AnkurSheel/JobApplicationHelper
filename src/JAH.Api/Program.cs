using System;

using Autofac;

using JAH.Data;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JAH.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (var container = IOCBuilder.Build())
                {
                    var webHost = BuildWebHost(container);
                    ProcessDb(webHost);
                    webHost.Run();
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
                return WebHost.CreateDefaultBuilder()
                              .UseStartup<Startup>()
                              .UseApplicationInsights()
                              .ConfigureServices(services => services.AddTransient(provider =>
                              {
                                  var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                                  var config = provider.GetRequiredService<IConfiguration>();
                                  var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Startup>>();
                                  return factory(hostingEnv, config);
                              }))
                              .Build();
            }
        }

        private static void ProcessDb(IWebHost webHost)
        {
            using (var serviceScope = webHost.Services.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<JobApplicationDbContext>();
                var dbSeeder = serviceScope.ServiceProvider.GetService<DbSeeder>();
                var env = serviceScope.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                context.UpdateDB();
                dbSeeder.SeedData(env.IsDevelopment());
            }
        }
    }
}
