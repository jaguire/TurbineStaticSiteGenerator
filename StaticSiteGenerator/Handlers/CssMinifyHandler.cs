#pragma warning disable S112 // General exceptions should never be thrown
using System;
using System.IO;
using System.Linq;
using NUglify;
using OutputColorizer;

namespace StaticSiteGenerator.Handlers
{
    public interface ICssMinifyHandler
    {
        void Process();
    }

    public class CssMinifyHandler : ICssMinifyHandler
    {
        private readonly AppSettings appSettings;

        public CssMinifyHandler(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public void Process()
        {
            if (!appSettings.MinifyCss)
                return;
            Console.WriteLine("Css Minify");

            // get files
            var inFiles = Directory.GetFiles(appSettings.Input, "*.css", SearchOption.AllDirectories)
                                   .Select(x => new FileInfo(x))
                                   .Where(x => !x.Name.StartsWith("_"))
                                   .ToList();
            Colorizer.WriteLine($"  Processing [White!{inFiles.Count}] files...");

            foreach (var inFile in inFiles)
            {
                if (appSettings.Verbose)
                    Colorizer.WriteLine($"    [DarkGray!{inFile.FullName.Replace(appSettings.Input, ".")}]");
                try
                {
                    // minify
                    var css = File.ReadAllText(inFile.FullName);
                    var result = Uglify.Css(css);

                    // save result
                    var outFile = new FileInfo(inFile.FullName.Replace(appSettings.Input, appSettings.Output));
                    outFile.Directory?.Create();
                    File.WriteAllText(outFile.FullName, result.Code);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error processing file: {inFile.FullName.Replace(appSettings.Input, ".")}", ex);
                }
            }
        }
    }
}