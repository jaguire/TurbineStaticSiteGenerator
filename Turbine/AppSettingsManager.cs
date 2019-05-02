using System;
using System.IO;
using OutputColorizer;
using YamlDotNet.Serialization;

namespace Turbine
{
    public interface IAppSettingsManager
    {
        IAppSettings GetAppSettings();
    }

    public class AppSettingsManager : IAppSettingsManager
    {
        private readonly ICommandLineOptions options;

        public AppSettingsManager(ICommandLineOptions options)
        {
            this.options = options;
        }

        public IAppSettings GetAppSettings()
        {
            Colorizer.WriteLine("Configuration");
            var appSettings = File.Exists(options.Config)
                ? new Deserializer().Deserialize<AppSettings>(File.ReadAllText(options.Config))
                : new AppSettings();

            // watch
            appSettings.Watch = appSettings.Watch || options.Watch;

            // input
            var input = new DirectoryInfo(appSettings.Input);
            Colorizer.WriteLine($"  Input Path: [DarkCyan!{input.FullName}]");
            if (!input.Exists)
            {
                Colorizer.WriteLine("[Red!    The input path could not be found.]");
                Environment.Exit(0);
            }
            appSettings.Input = input.FullName;

            // output
            var output = new DirectoryInfo(appSettings.Output);
            Colorizer.WriteLine($"  Output Path: [DarkCyan!{output.FullName}]");
            if (output.Exists)
                output.Delete(true);
            output.Create();
            appSettings.Output = output.FullName;

            return appSettings;
        }
    }
}