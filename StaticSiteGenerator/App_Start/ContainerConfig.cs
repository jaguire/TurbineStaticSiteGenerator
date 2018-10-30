using System.Linq;
using Autofac;

namespace StaticSiteGenerator
{
    public static class ContainerConfig
    {
        public static IContainer GetContainer(AppSettings appSettings)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(appSettings);

            builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                   .Where(t => t.IsClass && t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"))
                   .As(t => t.GetInterfaces().First(i => i.Name == $"I{t.Name}"));

            return builder.Build();
        }
    }
}
