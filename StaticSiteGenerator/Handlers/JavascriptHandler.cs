#pragma warning disable S112 // General exceptions should never be thrown
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUglify;
using OutputColorizer;

namespace StaticSiteGenerator.Handlers
{
    public interface IJavascriptHandler
    {
        void Process();
    }

    public class JavascriptHandler : IJavascriptHandler
    {
        private readonly AppSettings appSettings;

        public JavascriptHandler(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public void Process()
        {
            if (!appSettings.MinifyJs)
                return;
            Console.WriteLine("JavaScript Minify");

            // get files
            var inFiles = Directory.GetFiles(appSettings.Input, "*.js", SearchOption.AllDirectories)
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
                    // process
                    var js = ProcessFile(inFile);

                    // minify
                    var result = Uglify.Js(js);

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