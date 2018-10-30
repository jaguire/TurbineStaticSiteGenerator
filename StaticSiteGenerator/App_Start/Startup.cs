using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using Autofac;
using Fclp;
using OutputColorizer;

namespace StaticSiteGenerator
{
    public static class Startup
    {
        private static bool isWatch;

        public static ManualResetEvent Exit { get; private set; }

        private static int Main()
        {
            Exit = CreateExitEvent();
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                return CreateCustomAppDomainForRazorEngine();

            DisplayLogo();

            ParseCommandLineArguments();
            var appSettings = AppSettingsConfig.GetAppSettings(isWatch);
            var container = ContainerConfig.GetContainer(appSettings);
            container.Resolve<IProgram>().Run(isWatch);
            return 0;
        }

        private static void ParseCommandLineArguments()
        {
            var parser = new FluentCommandLineParser();

            parser.Setup<bool>('w', "watch")
                  .Callback(x => isWatch = x)
                  .WithDescription("Watch the input folder for changes.")
                  .SetDefault(false);

            parser.SetupHelp("?", "help")
                  .Callback(text => Console.WriteLine(text));

            var result = parser.Parse(Environment.GetCommandLineArgs());
            if (!result.HasErrors)
                return;

            Colorizer.WriteLine($"[Red!{result.ErrorText}]");
            parser.HelpOption.ShowHelp(parser.Options);
            Environment.Exit(1);
        }

        private static void DisplayLogo()
        {
            var version = typeof(Startup).Assembly.GetName().Version;
            Colorizer.WriteLine($"[Cyan!Turbine Static Site Generator v{version.Major}.{version.Minor}.{version.Build}]");
        }

        private static int CreateCustomAppDomainForRazorEngine()
        {
            var domain = AppDomain.CreateDomain("StaticSiteGeneratorDomain", null, AppDomain.CurrentDomain.SetupInformation, new PermissionSet(PermissionState.Unrestricted), new StrongName[0]);
            var exitCode = domain.ExecuteAssembly(Assembly.GetExecutingAssembly().Location);
            AppDomain.Unload(domain);
            return exitCode;
        }

        private static ManualResetEvent CreateExitEvent()
        {
            var exit = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Colorizer.WriteLine("[Yellow!Turbine stopped]");
                eventArgs.Cancel = true;
                exit.Set();
            };
            return exit;
        }
    }
}