using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUglify;
using OutputColorizer;

namespace StaticSiteGenerator.Handlers
{
    public class HtmlHandler : IFileHandler
    {
        private readonly AppSettings appSettings;
        private readonly IHandlerUtility util;

        public HtmlHandler(AppSettings appSettings, IHandlerUtility util)
        {
            this.appSettings = appSettings;
            this.util = util;
        }

        public void Process()
        {
            Console.WriteLine("Html");

            // load metadata
            var siteMeta = new Dictionary<string, object>();
            var metaFile = new FileInfo($"{appSettings.Input}/shared/meta.json");
            if (metaFile.Exists)
            {
                Colorizer.WriteLine($"  Meta: [DarkCyan!{metaFile.FullName.Replace(appSettings.Input, ".")}]");
                siteMeta = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(metaFile.FullName));
                if (appSettings.Verbose)
                    foreach (var m in siteMeta)
                        Colorizer.WriteLine($"    [DarkGray!{m.Key} = \"{m.Value}\"]");
            }

            // get files
            var inFiles = util.GetFiles("htm", "html");
            Colorizer.WriteLine($"  Processing [White!{inFiles.Count}] files...");

            foreach (var inFile in inFiles)
            {
                if (appSettings.Verbose)
                    Colorizer.WriteLine($"    [DarkGray!{inFile.FullName.Replace(appSettings.Input, ".")}]");
                try
                {
                    // compile
                    var contents = ProcessFile(inFile);

                    // separate page meta
                    var separator = new[] { $"{Environment.NewLine}---{Environment.NewLine}" };
                    var parts = contents.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    var meta = parts.Length == 1 ? "{}" : parts[0];
                    var html = parts.Length == 1 ? parts[0] : parts[1];

                    // inject metadata
                    var pageMeta = JsonConvert.DeserializeObject<Dictionary<string, object>>(meta);
                    foreach (var m in pageMeta)
                        html = html.Replace($"{{{{ {m.Key} }}}}", m.Value.ToString());
                    foreach (var m in siteMeta)
                        html = html.Replace($"{{{{ {m.Key} }}}}", m.Value.ToString());
                    if (appSettings.MinifyHtml)
                        html = Uglify.Html(html).Code;

                    // save contents
                    var outFile = new FileInfo(inFile.FullName.Replace(appSettings.Input, appSettings.Output));
                    outFile.Directory?.Create();
                    File.WriteAllText(outFile.FullName, html);
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
            var matches = Regex.Matches(html, "{# Include '([^']+)' #}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
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