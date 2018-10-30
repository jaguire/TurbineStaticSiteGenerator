using System;
using System.IO;
using System.Threading;
using OutputColorizer;
using StaticSiteGenerator.Handlers;

namespace StaticSiteGenerator
{
    public interface IProgram
    {
        void Run(bool isWatch);
    }

    public class Program : IProgram
    {
        private const int RollingDelayInSeconds = 2;

        private readonly AppSettings appSettings;
        private readonly IRazorHandler razor;
        private readonly IHtmlHandler html;
        private readonly IJavascriptHandler javascript;
        private readonly ISassHandler sass;
        private readonly ICssMinifyHandler cssMinify;
        private Timer watchTimer;

        public Program(AppSettings appSettings, IRazorHandler razor, IHtmlHandler html, IJavascriptHandler javascript, ISassHandler sass, ICssMinifyHandler cssMinify)
        {
            this.appSettings = appSettings;
            this.razor = razor;
            this.html = html;
            this.javascript = javascript;
            this.sass = sass;
            this.cssMinify = cssMinify;
        }

        public void Run(bool isWatch)
        {
            try
            {
                if (isWatch)
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
                }
                else
                {
                    Process();
                    Colorizer.WriteLine("[Green!Competed]");
                }
            }
            catch (Exception ex)
            {
                Colorizer.WriteLine("[Red!Error]");
                Console.WriteLine($"  {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  {ex.InnerException.Message}");
            }
#if DEBUG
            Console.ReadKey();
#endif
        }

        private void Process()
        {
            Colorizer.WriteLine("[Yellow!Processing...]");
            razor.Process();
            html.Process();
            javascript.Process();
            sass.Process();
            cssMinify.Process();
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
