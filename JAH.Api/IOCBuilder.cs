using Autofac;
using JAH.Data;
using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.Data.Repositories;
using JAH.Services.Interfaces;
using JAH.Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

public class IOCBuilder
{
    private static readonly LoggerFactory MyLoggerFactory =
            new LoggerFactory(new[] { new DebugLoggerProvider((_, level) => level >= LogLevel.Information) });

    internal static IContainer Build()
    {
        var builder = new ContainerBuilder();
        RegisterTypes(builder);
        return builder.Build();
    }

    private static void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType<JobApplicationService>().As<IJobApplicationService>();
        builder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplicationEntity>>().InstancePerLifetimeScope();
        DbContextOptions<JobApplicationDbContext> options = new DbContextOptionsBuilder<JobApplicationDbContext>()
                .UseSqlServer("Server = (localdb)\\mssqllocaldb; Database = JobApplicationData; Trusted_Connection = True;")
                .UseLoggerFactory(MyLoggerFactory)
                .Options;
        builder.RegisterType<JobApplicationDbContext>()
               .As<JobApplicationDbContext>()
               .WithParameter(new TypedParameter(typeof(DbContextOptions), options))
               .InstancePerLifetimeScope();
    }
}
