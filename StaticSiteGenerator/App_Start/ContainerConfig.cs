using System.Linq;
using Autofac;
using StaticSiteGenerator.Handlers;

namespace StaticSiteGenerator
{
    public static class ContainerConfig
    {
        public static IContainer GetContainer(AppSettings appSettings)
        {
            var builder = new ContainerBuilder();

            // app settings
            builder.RegisterInstance(appSettings);

            // handlers
            builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                   .Where(t => t.IsClass && t.GetInterfaces().Any(i => i.Name == "IFileHandler"))
                   .As<IFileHandler>();

            // default interfaces
            builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                   .Where(t => t.IsClass && t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"))
                   .As(t => t.GetInterfaces().First(i => i.Name == $"I{t.Name}"));

            return builder.Build();
        }
    }
}