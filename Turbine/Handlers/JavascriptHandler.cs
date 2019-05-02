using System;
using System.IO;
using System.Text.RegularExpressions;
using NUglify;
using OutputColorizer;

namespace Turbine.Handlers
{
    public class JavascriptHandler : IFileHandler
    {
        private readonly IAppSettings appSettings;
        private readonly IHandlerUtility util;

        public JavascriptHandler(IAppSettings appSettings, IHandlerUtility util)
        {
            this.appSettings = appSettings;
            this.util = util;
        }

        public void Process()
        {
            Console.WriteLine("JavaScript Minify");

            // get files
            var inFiles = util.GetFiles("js");
            Colorizer.WriteLine($"  Processing [White!{inFiles.Count}] files...");

            foreach (var inFile in inFiles)
            {
                if (appSettings.Verbose)
                    Colorizer.WriteLine($"    [DarkGray!{inFile.FullName.Replace(appSettings.Input, ".")}]");
                try
                {
                    // process
                    var js = ProcessFile(inFile);

                    // minify
                    if (appSettings.MinifyJs && !inFile.Name.ToLowerInvariant().EndsWith(".min.js"))
                        js = Uglify.Js(js).Code;

                    // save result
                    var outFile = new FileInfo(inFile.FullName.Replace(appSettings.Input, appSettings.Output));
                    outFile.Directory?.Create();
                    File.WriteAllText(outFile.FullName, js);
                }
                catch (Exception ex)
                {
                    throw util.ProcessingException(inFile, ex);
                }
            }
        }

        private string ProcessFile(FileInfo file)
        {
            // load contents
            var html = File.ReadAllText(file.FullName);

            // includes
            var matches = Regex.Matches(html, @"// Include '([^']+)'", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var include = new FileInfo($"{file.DirectoryName}/{match.Groups[1].Value}");
                if (appSettings.Verbose)
                    Colorizer.WriteLine($"      [DarkGray!{include.FullName.Replace(appSettings.Input, ".")}]");
                var includeContents = ProcessFile(include);
                html = html.Replace(match.Captures[0].Value, includeContents);
            }

            return html;
        }
    }
}