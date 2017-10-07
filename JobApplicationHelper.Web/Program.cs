using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobApplicationHelper.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();

            using (var container = builder.Build())
            {
                await StartWebServerAsync(container);
            }
        }

        private static async Task StartWebServerAsync(ILifetimeScope scope)
        {
            using (var webHostScope = scope.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
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
