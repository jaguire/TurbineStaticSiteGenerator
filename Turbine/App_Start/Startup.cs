using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using Autofac;
using OutputColorizer;

namespace Turbine
{
    public static class Startup
    {
        public static ManualResetEvent Exit { get; private set; }

        private static int Main()
        {
            Exit = CreateExitEvent();
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                return CreateCustomAppDomainForRazorEngine();

            DisplayLogo();

            ContainerConfig.GetContainer()
                           .Resolve<IProgram>()
                           .Run();
            return 0;
        }

        private static void DisplayLogo()
        {
            var version = typeof(Startup).Assembly.GetName().Version;
            Colorizer.WriteLine($"[Cyan!Turbine v{version.Major}.{version.Minor}.{version.Build}]");
        }

        private static int CreateCustomAppDomainForRazorEngine()
        {
            var domain = AppDomain.CreateDomain("TurbineDomain", null, AppDomain.CurrentDomain.SetupInformation, new PermissionSet(PermissionState.Unrestricted), new StrongName[0]);
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