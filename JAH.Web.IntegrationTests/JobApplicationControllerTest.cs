using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using JAH.Data;
using JAH.Data.Entities;
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
using Xunit;

namespace JAH.Web.IntegrationTests
{
    public class JobApplicationControllerTest : IDisposable
    {
        private readonly HttpClient _webClient;
        private readonly JobApplicationDbContext _jobApplicationDbContext;

        public JobApplicationControllerTest()
        {
            var options = new DbContextOptionsBuilder<JobApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _jobApplicationDbContext = new JobApplicationDbContext(options);

            _jobApplicationDbContext.JobApplications.Add(new JobApplicationEntity { Id = 1, CompanyName = "Company 1" });
            _jobApplicationDbContext.JobApplications.Add(new JobApplicationEntity { Id = 2, CompanyName = "Company 2" });
            _jobApplicationDbContext.JobApplications.Add(new JobApplicationEntity { Id = 3, CompanyName = "Company 3" });
            _jobApplicationDbContext.SaveChanges();

            HttpClient apiClient;
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<JobApplicationService>().As<IJobApplicationService>();
            containerBuilder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplication>>();
            containerBuilder.RegisterInstance(_jobApplicationDbContext).As<JobApplicationDbContext>().ExternallyOwned();

            using (var container = containerBuilder.Build())
            {
                using (var webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<Api.Startup>().AsSelf()))
                {
                    var fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath
                                                                 , ".."
                                                                 , ".."
                                                                 , ".."
                                                                 , ".."
                                                                 , "JAH.Api"));
                    var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Api.Startup>>();
                    var builder = new WebHostBuilder().UseKestrel()
                                                      .UseContentRoot(fullPath)
                                                      .UseEnvironment("Development")
                                                      .UseStartup<Api.Startup>()
                                                      .ConfigureServices(services => services.TryAddTransient(provider =>
                                                      {
                                                          var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                                                          var config = provider.GetRequiredService<IConfiguration>();
                                                          return factory(hostingEnv, config);
                                                      }));
                    var testServer = new TestServer(builder);
                    apiClient = testServer.CreateClient();
                }

                using (var webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
                {
                    var fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath
                                                                 , ".."
                                                                 , ".."
                                                                 , ".."
                                                                 , ".."
                                                                 , "JAH.Web"));
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
                    _webClient = testServer.CreateClient();
                }
            }
        }

        public void Dispose()
        {
            _webClient?.Dispose();
            _jobApplicationDbContext?.Dispose();
        }

        [Fact]
        public async Task ShouldReturnCorrectResponseWithAllJobApplications()
        {
            // Arrange
            const string expectedResponse = "[{\"name\":\"Company 1\"},{\"name\":\"Company 2\"},{\"name\":\"Company 3\"}]";

            // Act
            var response = await _webClient.GetAsync("/jobApplication");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(expectedResponse, responseData);
        }
    }
}
