using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;

namespace JAH.Web.IntegrationTests
{
    public class HelloControllerTest
    {
        private readonly HttpClient _webClient;

        public HelloControllerTest()
        {
            HttpClient apiClient;
            var containerBuilder = new ContainerBuilder();
            using (var container = containerBuilder.Build())
            {
                using (var webHostScope = container.BeginLifetimeScope(builder => builder.RegisterType<JAH.Api.Startup>().AsSelf()))
                {
                    var fullPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath
                                                                 , ".."
                                                                 , ".."
                                                                 , ".."
                                                                 , ".."
                                                                 , "JAH.Api"));
                    var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, JAH.Api.Startup>>();
                    var builder = new WebHostBuilder().UseKestrel()
                                                      .UseContentRoot(fullPath)
                                                      .UseEnvironment("Development")
                                                      .UseStartup<JAH.Api.Startup>()
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

        [Fact]
        public async Task ShouldReturnCorrectResponseWithAGreeting()
        {
            // Arrange

            // Act
            var response = await _webClient.GetAsync("/hello/ankur");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = response.Content.ReadAsStringAsync().Result;
            Assert.Equal("Hello ankur", responseData);
        }
    }
}
