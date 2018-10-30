using System;
using System.IO;
using NUglify;
using OutputColorizer;

namespace StaticSiteGenerator.Handlers
{
    public class StyleHandler : IFileHandler
    {
        private readonly AppSettings appSettings;
        private readonly IHandlerUtility util;

        public StyleHandler(AppSettings appSettings, IHandlerUtility util)
        {
            this.appSettings = appSettings;
            this.util = util;
        }

        public void Process()
        {
            Console.WriteLine("Css Minify");

            // get files
            var inFiles = util.GetFiles("css");
            Colorizer.WriteLine($"  Processing [White!{inFiles.Count}] files...");

            foreach (var inFile in inFiles)
            {
                if (appSettings.Verbose)
                    Colorizer.WriteLine($"    [DarkGray!{inFile.FullName.Replace(appSettings.Input, ".")}]");
                try
                {
                    // process
                    var css = File.ReadAllText(inFile.FullName);

                    // minify
                    if (appSettings.MinifyCss)
                        css = Uglify.Css(css).Code;

                    // save result
                    var outFile = new FileInfo(inFile.FullName.Replace(appSettings.Input, appSettings.Output));
                    outFile.Directory?.Create();
                    File.WriteAllText(outFile.FullName, css);
                }
                catch (Exception ex)
                {
                    throw util.ProcessingException(inFile, ex);
                }
            }
        }
    }
}