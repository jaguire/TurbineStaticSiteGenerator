#pragma warning disable S112 // General exceptions should never be thrown
using System;
using System.IO;
using System.Linq;
using OutputColorizer;
using SharpScss;

namespace StaticSiteGenerator.Handlers
{
    public interface ISassHandler
    {
        void Process();
    }

    public class SassHandler : ISassHandler
    {
        private readonly AppSettings appSettings;

        public SassHandler(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public void Process()
        {
            Console.WriteLine("Sass");
            Colorizer.WriteLine($"  Output Style: [DarkCyan!{appSettings.ScssOutputStyle}]");

            // get files
            var inFiles = Directory.GetFiles(appSettings.Input, "*.scss", SearchOption.AllDirectories)
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
                    var outFile = new FileInfo(inFile.FullName
                                                     .Replace(appSettings.Input, appSettings.Output)
                                                     .Replace(".scss", ".css"));
                    // preprocess
                    var options = new ScssOptions
                    {
                        InputFile = inFile.FullName,
                        OutputFile = outFile.FullName,
                        OutputStyle = appSettings.ScssOutputStyle
                    };
                    var result = Scss.ConvertFileToCss(inFile.FullName, options);

                    // save result
                    outFile.Directory?.Create();
                    File.WriteAllText(outFile.FullName, result.Css);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error processing file: {inFile.FullName.Replace(appSettings.Input, ".")}", ex);
                }
            }
        }
    }
}