using System.Collections.Generic;
using SharpScss;

namespace Turbine
{
    public interface IAppSettings
    {
        string Input { get; set; }
        string Output { get; set; }
        ScssOutputStyle ScssOutput { get; set; }
        bool MinifyHtml { get; set; }
        bool MinifyCss { get; set; }
        bool MinifyJs { get; set; }
        bool Verbose { get; set; }
        List<string> AdditionalStaticFiles { get; set; }

        bool Watch { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        public string Input { get; set; } = "./input";
        public string Output { get; set; } = "./output";
        public ScssOutputStyle ScssOutput { get; set; } = ScssOutputStyle.Expanded;
        public bool MinifyHtml { get; set; } = false;
        public bool MinifyCss { get; set; } = false;
        public bool MinifyJs { get; set; } = false;
        public bool Verbose { get; set; } = false;
        public List<string> AdditionalStaticFiles { get; set; } = new List<string>();

        public bool Watch { get; set; }
    }
}