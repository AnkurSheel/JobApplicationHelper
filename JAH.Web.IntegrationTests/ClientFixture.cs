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
        private readonly IContainer _container;
        private readonly ILifetimeScope _defaultScope;
        private HttpClient _apiClient;

        public ClientFixture()
        {
            var dbContextOptions = new DbContextOptionsBuilder<JobApplicationDbContext>()
                                   .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                   .EnableSensitiveDataLogging()
                                   .Options;

            var builder = new ContainerBuilder();
            builder.RegisterType<JobApplicationService>().As<IJobApplicationService>();
            builder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplicationEntity>>();
            builder.RegisterType<JobApplicationDbContext>()
                   .As<JobApplicationDbContext>()
                   .WithParameter(new TypedParameter(typeof(DbContextOptions), dbContextOptions))
                   .InstancePerLifetimeScope();

            _container = builder.Build();
            _defaultScope = _container.BeginLifetimeScope();

            SetupClients();
        }

        public HttpClient WebClient { get; private set; }

        public JobApplicationDbContext JobApplicationDbContext
        {
            get { return _defaultScope.Resolve<JobApplicationDbContext>(); }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void DetachAllEntities()
        {
            JobApplicationDbContext.JobApplications.RemoveRange(JobApplicationDbContext.JobApplications);
            JobApplicationDbContext.SaveChanges();
        }

        public void SetupAuthentication()
        {
            _apiClient.DefaultRequestHeaders.Add(AuthenticatedTestRequestMiddleware.TestingHeader,
                                                 AuthenticatedTestRequestMiddleware.TestingHeaderValue);

            _apiClient.DefaultRequestHeaders.Add("my-name", "abcde");
            _apiClient.DefaultRequestHeaders.Add("my-id", "12345");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _apiClient?.Dispose();
                _defaultScope?.Dispose();
                _container?.Dispose();
                WebClient?.Dispose();
            }
        }

        private void SetupClients()
        {
            using (ILifetimeScope webHostScope = _container.BeginLifetimeScope(builder => builder.RegisterType<TestApiServerStartup>().AsSelf()))
            {
                string fullPath =
                    Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", "..", "..", "JAH.Api"));

                var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, TestApiServerStartup>>();
                IWebHostBuilder builder = new WebHostBuilder().UseKestrel()
                                                              .UseContentRoot(fullPath)
                                                              .UseEnvironment("Testing")
                                                              .UseStartup<TestApiServerStartup>()
                                                              .ConfigureServices(services =>
                                                                                     services.TryAddTransient(provider =>
                                                                                                                  SetupStartup(provider, factory)));
                var testServer = new TestServer(builder);
                _apiClient = testServer.CreateClient();
            }

            using (ILifetimeScope webHostScope = _container.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
            {
                string fullPath =
                    Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", "..", "..", "JAH.Web"));

                var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Startup>>();
                IWebHostBuilder builder = new WebHostBuilder().UseKestrel()
                                                              .UseContentRoot(fullPath)
                                                              .UseEnvironment("Testing")
                                                              .UseStartup<Startup>()
                                                              .ConfigureServices(services =>
                                                                                     services.AddTransient(provider =>
                                                                                                               SetupStartup(provider, factory)))
                                                              .ConfigureServices(services => services.TryAddSingleton(_apiClient));

                var testServer = new TestServer(builder);
                WebClient = testServer.CreateClient();
            }
        }

        private Startup SetupStartup(IServiceProvider provider, Func<IHostingEnvironment, IConfiguration, Startup> factory)
        {
            var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
            var config = provider.GetRequiredService<IConfiguration>();
            return factory(hostingEnv, config);
        }

        private TestApiServerStartup SetupStartup(IServiceProvider provider, Func<IHostingEnvironment, IConfiguration, TestApiServerStartup> factory)
        {
            var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
            var config = provider.GetRequiredService<IConfiguration>();
            return factory(hostingEnv, config);
        }
    }
}
