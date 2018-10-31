using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OutputColorizer;

namespace StaticSiteGenerator.Handlers
{
    public class StaticFileHandler : IFileHandler
    {
        private readonly AppSettings appSettings;
        private readonly IHandlerUtility util;

        public StaticFileHandler(AppSettings appSettings, IHandlerUtility util)
        {
            this.appSettings = appSettings;
            this.util = util;
        }

        public void Process()
        {
            Console.WriteLine("Static Files");

            // get files
            var extensions = new List<string> { "jpg", "jpeg", "gif", "png", "ico", "txt", "xml", "pdf" };
            extensions.AddRange(appSettings.AdditionalStaticFiles ?? new string[0]);
            var inFiles = util.GetFiles(extensions.ToArray());
            inFiles.AddRange(Directory.EnumerateFiles(appSettings.Input, "*.", SearchOption.AllDirectories)
                                      .Where(x => Path.GetFileName(x)?.StartsWith("_") == false)
                                      .Select(x => new FileInfo(x)));

            Colorizer.WriteLine($"  Processing [White!{inFiles.Count}] files...");

            foreach (var inFile in inFiles)
            {
                if (appSettings.Verbose)
                    Colorizer.WriteLine($"    [DarkGray!{inFile.FullName.Replace(appSettings.Input, ".")}]");
                try
                {
                    // copy
                    var outFile = new FileInfo(inFile.FullName.Replace(appSettings.Input, appSettings.Output));
                    outFile.Directory?.Create();
                    File.Copy(inFile.FullName, outFile.FullName);
                }
                catch (Exception ex)
                {
                    throw util.ProcessingException(inFile, ex);
                }
            }
        }
    }
}