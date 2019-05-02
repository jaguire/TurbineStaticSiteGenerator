using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Turbine
{
    public interface IHandlerUtility
    {
        List<FileInfo> GetFiles(params string[] extensions);
        Exception ProcessingException(FileInfo file, Exception ex);
    }

    public class HandlerUtility : IHandlerUtility
    {
        private readonly IAppSettings appSettings;

        public HandlerUtility(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public List<FileInfo> GetFiles(params string[] extensions)
        {
            return extensions.SelectMany(ext =>
                Directory.EnumerateFiles(appSettings.Input, "*", SearchOption.AllDirectories)
                         .Where(x => string.Equals(Path.GetExtension(x), $".{ext}", StringComparison.InvariantCultureIgnoreCase))
                         .Where(x => Path.GetFileName(x)?.StartsWith("_") == false)
                         .Select(x => new FileInfo(x))).ToList();
        }

        public Exception ProcessingException(FileInfo file, Exception ex)
        {
            return new Exception($"Error processing file: {file.FullName.Replace(appSettings.Input, ".")}", ex);
        }
    }
}