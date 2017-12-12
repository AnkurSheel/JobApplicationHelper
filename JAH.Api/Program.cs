using System;
using System.Threading.Tasks;
using Autofac;
using JAH.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JAH.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (IContainer container = IOCBuilder.Build())
            {
                SeedDatabase(container);

                await StartWebServerAsync(container);
            }
        }

        private static void SeedDatabase(IContainer container)
        {
            try
            {
                var context = container.Resolve<JobApplicationDbContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static async Task StartWebServerAsync(ILifetimeScope scope)
        {
            using (ILifetimeScope webHostScope = scope.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
            {
                await WebHost.CreateDefaultBuilder()
                             .UseStartup<Startup>()
                             .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning))
                             .ConfigureServices(services => services.AddTransient(provider =>
                                                                                  {
                                                                                      var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                                                                                      var config = provider.GetRequiredService<IConfiguration>();
                                                                                      var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Startup>>();
                                                                                      return factory(hostingEnv, config);
                                                                                  }))

                             .Build()
                             .RunAsync();
            }
        }
    }
}
