using System;
using System.Threading.Tasks;
using Autofac;
using JAH.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JAH.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (IContainer container = IOCBuilder.Build())
                {
                    SeedDatabase(container);

                    StartWebServerAsync(container).Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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
