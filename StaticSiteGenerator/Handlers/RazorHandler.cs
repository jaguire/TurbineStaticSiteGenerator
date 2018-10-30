#pragma warning disable S112 // General exceptions should never be thrown
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUglify;
using OutputColorizer;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace StaticSiteGenerator.Handlers
{
    public interface IRazorHandler
    {
        void Process();
    }

    public class RazorHandler : IRazorHandler
    {
        private readonly AppSettings appSettings;

        public RazorHandler(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public void Process()
        {
            Console.WriteLine("Razor");

            // metadata
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
            var inFiles = Directory.GetFiles(appSettings.Input, "*.cshtml", SearchOption.AllDirectories)
                                   .Select(x => new FileInfo(x))
                                   .Where(x => !x.Name.StartsWith("_"))
                                   .ToList();
            Colorizer.WriteLine($"  Processing [White!{inFiles.Count}] files...");

            // create razor engine
            var razor = RazorEngineService.Create(new TemplateServiceConfiguration
            {
                ////BaseTemplateType = typeof(SupportTemplateBase<>),
                TemplateManager = new ResolvePathTemplateManager(new[] { $"{appSettings.Input}/shared" })
            });
            foreach (var inFile in inFiles)
            {
                if (appSettings.Verbose)
                    Colorizer.WriteLine($"    [DarkGray!{inFile.FullName.Replace(appSettings.Input, ".")}]");
                try
                {
                    // compile
                    var html = razor.RunCompile(inFile.FullName, null, null, new DynamicViewBag(siteMeta));
                    if (appSettings.MinifyHtml)
                        html = Uglify.Html(html).Code;

                    // save contents
                    var outFile = new FileInfo(inFile.FullName
                                                     .Replace(appSettings.Input, appSettings.Output)
                                                     .Replace(".cshtml", ".html"));
                    outFile.Directory?.Create();
                    File.WriteAllText(outFile.FullName, html);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error processing file: {inFile.FullName.Replace(appSettings.Input, ".")}", ex);
                }
            }
        }
    }
}
