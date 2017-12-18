using Autofac;
using JAH.Data;
using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.Data.Repositories;
using JAH.Services.Interfaces;
using JAH.Services.Services;
using Microsoft.EntityFrameworkCore;

public class IOCBuilder
{
    internal static IContainer Build()
    {
        var builder = new ContainerBuilder();
        RegisterTypes(builder);
        return builder.Build();
    }

    private static void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType<JobApplicationService>().As<IJobApplicationService>();
        builder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplicationEntity>>();
        var options = new DbContextOptionsBuilder<JobApplicationDbContext>()
            .UseSqlServer("Server = (localdb)\\mssqllocaldb; Database = JobApplicationData; Trusted_Connection = True;")
            .Options;
        builder.RegisterType<JobApplicationDbContext>()
               .As<JobApplicationDbContext>()
               .WithParameter(new TypedParameter(typeof(DbContextOptions), options));
    }
}
