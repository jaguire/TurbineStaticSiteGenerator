using System;
using System.IO;
using Newtonsoft.Json;
using OutputColorizer;

namespace StaticSiteGenerator
{
    public static class AppSettingsConfig
    {
        public static AppSettings GetAppSettings()
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
