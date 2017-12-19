﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace JAH.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = new ContainerBuilder();

                using (var container = builder.Build())
                {
                    StartWebServerAsync(container).Wait();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static async Task StartWebServerAsync(ILifetimeScope scope)
        {
            using (var webHostScope = scope.BeginLifetimeScope(builder => builder.RegisterType<Startup>().AsSelf()))
            {
                await WebHost.CreateDefaultBuilder()
                             .UseStartup<Startup>()
                             .UseContentRoot(Directory.GetCurrentDirectory())
                             .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning))
                             .ConfigureServices(services => services.TryAddTransient(provider =>
                             {
                                 var hostingEnv = provider.GetRequiredService<IHostingEnvironment>();
                                 var config = provider.GetRequiredService<IConfiguration>();
                                 var factory = webHostScope.Resolve<Func<IHostingEnvironment, IConfiguration, Startup>>();
                                 return factory(hostingEnv, config);
                             }))
                             .ConfigureServices(services => services.TryAddSingleton(provider =>
                             {
                                 var config = provider.GetRequiredService<IConfiguration>();
                                 var uri = config.GetSection("ApiOptions:BaseUri").Value;
                                 var client = new HttpClient { BaseAddress = new Uri(uri) };
                                 client.DefaultRequestHeaders.Accept.Clear();
                                 client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                 return client;
                             }))
                             .Build()
                             .RunAsync();
            }
        }
    }
}
