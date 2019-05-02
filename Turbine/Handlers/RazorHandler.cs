using System;
using System.Collections.Generic;
using System.IO;
using NUglify;
using OutputColorizer;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using YamlDotNet.Serialization;

namespace Turbine.Handlers
{
    public class RazorHandler : IFileHandler
    {
        private readonly IAppSettings appSettings;
        private readonly IHandlerUtility util;

        public RazorHandler(IAppSettings appSettings, IHandlerUtility util)
        {
            this.appSettings = appSettings;
            this.util = util;
        }

        public void Process()
        {
            Console.WriteLine("Razor");

            // metadata
            var siteMeta = new Dictionary<string, object>();
            var metaFile = new FileInfo($"{appSettings.Input}/shared/meta.yaml");
            if (metaFile.Exists)
            {
                Colorizer.WriteLine($"  Meta: [DarkCyan!{metaFile.FullName.Replace(appSettings.Input, ".")}]");
                siteMeta = new Deserializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(metaFile.FullName));
                if (appSettings.Verbose)
                    foreach (var m in siteMeta)
                        Colorizer.WriteLine($"    [DarkGray!{m.Key} = \"{m.Value}\"]");
            }

            // get files
            var inFiles = util.GetFiles("cshtml");
            Colorizer.WriteLine($"  Processing [White!{inFiles.Count}] files...");

            // create razor engine
            var razor = RazorEngineService.Create(new TemplateServiceConfiguration
            {
                ////BaseTemplateType = typeof(SupportTemplateBase<>),
                TemplateManager = new ResolvePathTemplateManager(new[] { $"{appSettings.Input}/shared" }),
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
                    throw util.ProcessingException(inFile, ex);
                }
            }
        }
    }
}
