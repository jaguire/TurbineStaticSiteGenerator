using System;
using System.IO;
using OutputColorizer;
using SharpScss;

namespace Turbine.Handlers
{
    public class SassHandler : IFileHandler
    {
        private readonly IAppSettings appSettings;
        private readonly IHandlerUtility util;

        public SassHandler(IAppSettings appSettings, IHandlerUtility util)
        {
            this.appSettings = appSettings;
            this.util = util;
        }

        public void Process()
        {
            Console.WriteLine("Sass");
            Colorizer.WriteLine($"  Output Style: [DarkCyan!{appSettings.ScssOutput}]");

            // get files
            var inFiles = util.GetFiles("scss", "sass");
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
                        OutputStyle = appSettings.ScssOutput,
                    };
                    var result = Scss.ConvertFileToCss(inFile.FullName, options);

                    // save result
                    outFile.Directory?.Create();
                    File.WriteAllText(outFile.FullName, result.Css);
                }
                catch (Exception ex)
                {
                    throw util.ProcessingException(inFile, ex);
                }
            }
        }
    }
}