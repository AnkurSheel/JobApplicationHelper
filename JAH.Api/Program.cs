using System;
using System.Threading.Tasks;
using Autofac;
using JAH.Data;
using JAH.Data.Interfaces;
using JAH.Data.Repositories;
using JAH.DomainModels;
using JAH.Services.Interfaces;
using JAH.Services.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JAH.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<JobApplicationService>().As<IJobApplicationService>();
            builder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplication>>();
            var options = new DbContextOptionsBuilder<JobApplicationDbContext>()
                .UseSqlServer("Server = (localdb)\\mssqllocaldb; Database = JobApplicationData; Trusted_Connection = True;")
                .Options;
            builder.RegisterType<JobApplicationDbContext>().As<JobApplicationDbContext>().WithParameter(new TypedParameter(typeof(DbContextOptions), options));

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
