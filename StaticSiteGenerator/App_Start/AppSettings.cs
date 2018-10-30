﻿using SharpScss;

namespace StaticSiteGenerator
{
    public class AppSettings
    {
        public string Input { get; set; } = "./input";
        public string Output { get; set; } = "./output";
        public ScssOutputStyle ScssOutputStyle { get; set; } = ScssOutputStyle.Compressed;
        public bool MinifyHtml { get; set; }
        public bool MinifyCss { get; set; }
        public bool MinifyJs { get; set; }
        public bool Verbose { get; set; }
    }
}