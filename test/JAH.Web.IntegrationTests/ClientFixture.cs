using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

using Autofac;

using JAH.Data;
using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.Data.Repositories;
using JAH.Helper;
using JAH.Logger;
using JAH.Services.Interfaces;
using JAH.Services.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;

using Xunit.Abstractions;

namespace JAH.Web.IntegrationTests
{
    public class ClientFixture : IDisposable
    {
        private readonly IContainer _container;

        private HttpClient _apiClient;

        private TestServer _testServer;

        public ClientFixture()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<JobApplicationService>().As<IJobApplicationService>();
            builder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplicationEntity>>();
            builder.RegisterType<FakeJahLogger>().As<IJahLogger>().AsSelf().SingleInstance();
            _container = builder.Build();

            SetupClients();
        }

        public JobApplicationDbContext Context
        {
            get
            {
                return Services.GetService(typeof(JobApplicationDbContext)) as JobApplicationDbContext;
            }
        }

        public IServiceProvider Services
        {
            get
            {
                return _testServer.Host.Services;
            }
        }

        public HttpClient WebClient { get; private set; }

        public void ClearAuthentication()
        {
            _apiClient.DefaultRequestHeaders.Clear();
        }

        public void DetachAllEntities()
        {
            foreach (EntityEntry<JobApplicationEntity> dbEntityEntry in Context.ChangeTracker.Entries<JobApplicationEntity>().ToList())
            {
                if (dbEntityEntry.Entity != null)
                {
                    dbEntityEntry.State = EntityState.Detached;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void EmptyDatabase()
        {
            try
            {
                DetachAllEntities();
                Context.JobApplications.RemoveRange(Context.JobApplications.ToList());
                Context.Users.RemoveRange(Context.Users.ToList());

                Context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void SetupJwtAuthentication()
        {
            if (_apiClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                return;
            }

            var gen = new TokenGenerator(new TokenOptions("API Test",
                                                          "Test",
                                                          new SymmetricSecurityKey(Encoding.UTF8.GetBytes("VeryLongSecureString12345")),
                                                          1));
            _apiClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {gen.GenerateAccessToken("abcde", new List<Claim>())}");
        }

        public void SetupCookieAuthentication()
        {
            _apiClient.DefaultRequestHeaders.Add(AuthenticatedTestRequestMiddleware.TestingHeader,
                                                 AuthenticatedTestRequestMiddleware.TestingHeaderValue);

            _apiClient.DefaultRequestHeaders.Add("my-name", "abcde");
            _apiClient.DefaultRequestHeaders.Add("my-id", "12345");
        }

        public void SetupLogger(ITestOutputHelper output)
        {
            using (var webHostScope = _container.BeginLifetimeScope())
            {
                var logger = webHostScope.Resolve<FakeJahLogger>();
                logger.OutputHelper = output;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _testServer?.Dispose();
                _apiClient?.Dispose();
                _container?.Dispose();
                WebClient?.Dispose();
            }
        }

        private static TestWebServerStartup SetupStartup(IServiceProvider provider,
                                                         Func<IHostingEnvironment, IConfiguration, TestWebServerStartup> factory)
        {
            var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
            var config = provider.GetRequiredService<IConfiguration>();
            return factory(hostingEnv, config);
        }

        private static TestApiServerStartup SetupStartup(IServiceProvider provider,
                                                         Func<IHostingEnvironment, IConfiguration, TestApiServerStartup> factory)
        {
            var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
            var config = provider.GetRequiredService<IConfiguration>();
            return factory(hostingEnv, config);
        }

        private void SetupClients()
        {
            const string Environment = "Testing";
            using (var webHostScope = _container.BeginLifetimeScope(builder => builder.RegisterType<TestApiServerStartup>().AsSelf()))
            {
                var fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath,
                                                             "..",
                                                             "..",
                                                             "..",
                                                             "..",
                                                             "..",
                                                             "src",
                                                             "JAH.Api"));

                var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, TestApiServerStartup>>();
                var builder = new WebHostBuilder().UseKestrel()
                                                  .UseContentRoot(fullPath)
                                                  .UseEnvironment(Environment)
                                                  .UseStartup<TestApiServerStartup>()
                                                  .ConfigureAppConfiguration((builderContext, config) =>
                                                  {
                                                      var env = builderContext.HostingEnvironment;

                                                      config.AddJsonFile("appsettings.json", false, true)
                                                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
                                                  })
                                                  .ConfigureServices(services =>
                                                                         services.TryAddTransient(provider => SetupStartup(provider, factory)));
                _testServer = new TestServer(builder);
                _apiClient = _testServer.CreateClient();
            }

            using (var webHostScope = _container.BeginLifetimeScope(builder => builder.RegisterType<TestWebServerStartup>().AsSelf()))
            {
                var fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath,
                                                             "..",
                                                             "..",
                                                             "..",
                                                             "..",
                                                             "..",
                                                             "src",
                                                             "JAH.Web"));

                var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, TestWebServerStartup>>();
                var builder = new WebHostBuilder().UseKestrel()
                                                  .UseContentRoot(fullPath)
                                                  .UseEnvironment(Environment)
                                                  .UseStartup<TestWebServerStartup>()
                                                  .ConfigureServices(services => services.AddTransient(provider => SetupStartup(provider, factory)))
                                                  .ConfigureServices(services => services.TryAddSingleton(_apiClient));

                var testServer = new TestServer(builder);
                WebClient = testServer.CreateClient();
            }
        }
    }
}
