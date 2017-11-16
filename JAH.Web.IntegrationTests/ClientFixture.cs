using System;
using System.IO;
using System.Net.Http;
using Autofac;
using JAH.Data;
using JAH.Data.Interfaces;
using JAH.Data.Repositories;
using JAH.DomainModels;
using JAH.Services.Interfaces;
using JAH.Services.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;

namespace JAH.Web.IntegrationTests
{
    public class ClientFixture : IDisposable
    {
        public ClientFixture()
        {
            var dbContextOptions = new DbContextOptionsBuilder<JobApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            JobApplicationDbContext = new JobApplicationDbContext(dbContextOptions);


            HttpClient apiClient;
            Builder = new ContainerBuilder();
            Builder.RegisterType<JobApplicationService>().As<IJobApplicationService>();
            Builder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplication>>();
            Builder.RegisterInstance(JobApplicationDbContext).As<JobApplicationDbContext>().ExternallyOwned();

            using (IContainer container = Builder.Build())
            {
                using (ILifetimeScope webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<Api.Startup>().AsSelf()))
                {
                    string fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", "..", "..", "JAH.Api"));
                    var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Api.Startup>>();
                    IWebHostBuilder builder = new WebHostBuilder().UseKestrel()
                                                                  .UseContentRoot(fullPath)
                                                                  .UseEnvironment("Development")
                                                                  .UseStartup<Api.Startup>()
                                                                  .ConfigureServices(services => services.TryAddTransient(provider =>
                                                                  {
                                                                      var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                                                                      var config = ServiceProviderServiceExtensions.GetRequiredService<IConfiguration>(provider);
                                                                      return factory(hostingEnv, config);
                                                                  }));
                    var testServer = new TestServer(builder);
                    apiClient = testServer.CreateClient();
                }

                using (ILifetimeScope webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
                {
                    string fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", "..", "..", "JAH.Web"));
                    var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Startup>>();
                    var builder = new WebHostBuilder().UseKestrel()
                                                      .UseContentRoot(fullPath)
                                                      .UseEnvironment("Development")
                                                      .UseStartup<Startup>()
                                                      .ConfigureServices(services => services.AddTransient(provider =>
                                                      {
                                                          var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                                                          var config = provider.GetRequiredService<IConfiguration>();
                                                          return factory(hostingEnv, config);
                                                      }))
                                                      .ConfigureServices(services => services.TryAddSingleton(apiClient));

                    var testServer = new TestServer(builder);
                    WebClient = testServer.CreateClient();
                }
            }
        }

        public HttpClient WebClient { get; private set; }

        public ContainerBuilder Builder { get; }

        public JobApplicationDbContext JobApplicationDbContext { get; }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                WebClient?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
