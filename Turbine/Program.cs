using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OutputColorizer;

namespace Turbine
{
    public interface IProgram
    {
        void Run();
    }

    public class Program : IProgram
    {
        private const int RollingDelayInSeconds = 2;

        private readonly IAppSettings appSettings;
        private readonly IEnumerable<IFileHandler> fileHandlers;
        private readonly IWebServer webServer;
        private Timer watchTimer;

        public Program(IAppSettings appSettings, IEnumerable<IFileHandler> fileHandlers, IWebServer webServer)
        {
            this.appSettings = appSettings;
            this.fileHandlers = fileHandlers;
            this.webServer = webServer;
        }

        public void Run()
        {
            try
            {
                Process();

                if (appSettings.Watch)
                {
                    watchTimer = new Timer(OnWatchTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
                    var watcher = new FileSystemWatcher
                    {
                        IncludeSubdirectories = true,
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                        Path = appSettings.Input,
                    };
                    watcher.Created += FileChanged;
                    watcher.Changed += FileChanged;
                    watcher.Deleted += FileChanged;
                    watcher.Renamed += FileChanged;
                    watcher.Error += FileWatcherError;
                    watcher.EnableRaisingEvents = true;

                    Colorizer.WriteLine($"[Green!Watch mode started ({RollingDelayInSeconds} second delay)]");

                    webServer.Start();
                }
                else
                {
                    Colorizer.WriteLine("[Green!Completed]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Colorizer.WriteLine("[Red!ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR ERROR]");
                Console.WriteLine();
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine($"{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"{ex.InnerException.Message}");
                    Console.WriteLine($"{ex.InnerException.StackTrace}");
                }
            }
#if DEBUG
            Console.ReadKey();
#endif
        }

        private void Process()
        {
            Colorizer.WriteLine("[Yellow!Processing...]");
            foreach (var handler in fileHandlers)
                handler.Process();
            Colorizer.WriteLine("[Yellow!Completed.]");
        }

        private void OnWatchTimerCallback(object state)
        {
            Process();
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            if (appSettings.Verbose)
                Colorizer.WriteLine("[DarkGray!Changes detected!]");
            watchTimer.Change(TimeSpan.FromSeconds(RollingDelayInSeconds), Timeout.InfiniteTimeSpan);
        }

        private static void FileWatcherError(object sender, ErrorEventArgs e)
        {
            Colorizer.WriteLine("[Red!Error]");
            Console.WriteLine($"  {e.GetException().Message}");
            Environment.Exit(1);
        }
    }
}