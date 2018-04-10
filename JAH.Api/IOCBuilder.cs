using Autofac;

using JAH.Data.Entities;
using JAH.Data.Interfaces;
using JAH.Data.Repositories;
using JAH.Services.Interfaces;
using JAH.Services.Services;

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
        builder.RegisterType<JobApplicationRepository>().As<IRepository<JobApplicationEntity>>().InstancePerLifetimeScope();
    }
}
