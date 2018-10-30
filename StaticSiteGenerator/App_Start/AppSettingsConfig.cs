using System;
using System.IO;
using Newtonsoft.Json;
using OutputColorizer;

namespace StaticSiteGenerator
{
    public static class AppSettingsConfig
    {
        public static AppSettings GetAppSettings(bool isWatch)
        {
            Colorizer.WriteLine("Configuration");
            var appSettings = File.Exists("turbine.json")
                ? JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText("turbine.json"))
                : new AppSettings();

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
            DirectoryInfo output;
            if (isWatch)
            {
                output = new DirectoryInfo(appSettings.Watch);
                appSettings.Output = appSettings.Watch = output.FullName;
                Colorizer.WriteLine($"  Watch Path: [DarkCyan!{output.FullName}]");
            }
            else
            {
                output = new DirectoryInfo(appSettings.Output);
                appSettings.Output = output.FullName;
                Colorizer.WriteLine($"  Output Path: [DarkCyan!{output.FullName}]");
            }
            if (output.Exists)
                output.Delete(true);
            output.Create();

            return appSettings;
        }
    }
}
