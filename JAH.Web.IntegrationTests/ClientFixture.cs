using System;
using System.IO;
using System.Net.Http;
using Autofac;
using JAH.Data;
using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.Data.Repositories;
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
        private readonly ContainerBuilder _builder;

        public ClientFixture()
        {
            DbContextOptions<JobApplicationDbContext> dbContextOptions =
                new DbContextOptionsBuilder<JobApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            JobApplicationDbContext = new JobApplicationDbContext(dbContextOptions);

            _builder = new ContainerBuilder();
            _builder.RegisterType<JobApplicationService>().As<IJobApplicationService>();
            _builder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplicationEntity>>();
            _builder.RegisterInstance(JobApplicationDbContext).As<JobApplicationDbContext>().ExternallyOwned();

            SetupClients();
        }

        public HttpClient WebClient { get; private set; }

        public JobApplicationDbContext JobApplicationDbContext { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                WebClient?.Dispose();
            }
        }

        private void SetupClients()
        {
            HttpClient apiClient;

            using (IContainer container = _builder.Build())
            {
                using (ILifetimeScope webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<Api.Startup>().AsSelf()))
                {
                    string fullPath =
                        Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", "..", "..", "JAH.Api"));

                    var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Api.Startup>>();
                    IWebHostBuilder builder = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(fullPath)
                        .UseEnvironment("Development")
                        .UseStartup<Api.Startup>()
                        .ConfigureServices(services => services.TryAddTransient(provider => SetupStartup(provider, factory)));
                    var testServer = new TestServer(builder);
                    apiClient = testServer.CreateClient();
                }

                using (ILifetimeScope webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
                {
                    string fullPath =
                        Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", "..", "..", "JAH.Web"));

                    var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Startup>>();
                    IWebHostBuilder builder = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(fullPath)
                        .UseEnvironment("Development")
                        .UseStartup<Startup>()
                        .ConfigureServices(services => services.AddTransient(provider => SetupStartup(provider, factory)))
                        .ConfigureServices(services => services.TryAddSingleton(apiClient));

                    var testServer = new TestServer(builder);
                    WebClient = testServer.CreateClient();
                }
            }
        }

        private Startup SetupStartup(IServiceProvider provider, Func<IHostingEnvironment, IConfiguration, Startup> factory)
        {
            var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
            var config = provider.GetRequiredService<IConfiguration>();
            return factory(hostingEnv, config);
        }

        private Api.Startup SetupStartup(IServiceProvider provider, Func<IHostingEnvironment, IConfiguration, Api.Startup> factory)
        {
            var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
            var config = provider.GetRequiredService<IConfiguration>();
            return factory(hostingEnv, config);
        }
    }
}
