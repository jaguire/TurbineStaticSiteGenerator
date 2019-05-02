using System;
using System.Linq;
using Autofac;
using CommandLine;

namespace Turbine
{
    public static class ContainerConfig
    {
        public static IContainer GetContainer()
        {
            var builder = new ContainerBuilder();

            // handlers
            builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                   .Where(t => t.IsClass && t.GetInterfaces().Any(i => i.Name == "IFileHandler"))
                   .As<IFileHandler>();

            // default interfaces
            builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                   .Where(t => t.IsClass && t.GetInterfaces().Any(i => i.Name == $"I{t.Name}"))
                   .As(t => t.GetInterfaces().First(i => i.Name == $"I{t.Name}"));

            // Command line options
            Parser.Default.ParseArguments<CommandLineOptions>(Environment.GetCommandLineArgs())
                  .WithParsed(options => builder.RegisterInstance(options).As<ICommandLineOptions>())
                  .WithNotParsed(x => Environment.Exit(0));

            // app settings
            builder.Register(c => c.Resolve<IAppSettingsManager>().GetAppSettings()).InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}