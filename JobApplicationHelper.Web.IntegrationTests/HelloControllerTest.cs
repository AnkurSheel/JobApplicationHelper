using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobApplicationHelper.Web.IntegrationTests
{
    public class HelloControllerTest
    {
        private readonly HttpClient _apiClient;

        public HelloControllerTest()
        {
            var containerBuilder = new ContainerBuilder();
            using (var container = containerBuilder.Build())
            {
                using (var webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<Api.Startup>().AsSelf()))
                {
                    var fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath,
                                                                 "..",
                                                                 "..",
                                                                 "..",
                                                                 "..",
                                                                 "JobApplicationHelper.Api"));
                    var builder = new WebHostBuilder().UseKestrel()
                                                      .UseContentRoot(fullPath)
                                                      .UseEnvironment("Development")
                                                      .UseStartup<Api.Startup>()
                                                      .ConfigureServices(services => services.TryAddTransient(provider =>
                                                      {
                                                          var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                                                          var config = provider.GetRequiredService<IConfiguration>();
                                                          var factory =
                                                              webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Api.Startup>>();
                                                          return factory(hostingEnv, config);
                                                      }));
                    var testServer = new TestServer(builder);
                    _apiClient = testServer.CreateClient();
                }
            }
        }

        [Fact]
        public async Task ShouldReturnCorrectResponseWithAGreeting()
        {
            // Arrange
            var containerBuilder = new ContainerBuilder();
            using (var container = containerBuilder.Build())
            {
                using (var webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
                {
                    var fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath,
                                                                 "..",
                                                                 "..",
                                                                 "..",
                                                                 "..",
                                                                 "JobApplicationHelper.Web"));
                    var builder = new WebHostBuilder().UseKestrel()
                                                      .UseContentRoot(fullPath)
                                                      .UseEnvironment("Development")
                                                      .UseStartup<Startup>()
                                                      .ConfigureServices(services => services.AddTransient(provider =>
                                                      {
                                                          var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                                                          var config = provider.GetRequiredService<IConfiguration>();
                                                          var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Startup>>();
                                                          return factory(hostingEnv, config);
                                                      }))
                                                      .ConfigureServices(services => services.TryAddSingleton(_apiClient));

                    var testServer = new TestServer(builder);
                    var client = testServer.CreateClient();

                    // Act
                    var response = await client.GetAsync("/hello/ankur");

                    // Assert
                    response.EnsureSuccessStatusCode();
                    var responseData = response.Content.ReadAsStringAsync().Result;
                    Assert.Equal("Hello ankur", responseData);
                }
            }
        }
    }
}