using CommandLine;

namespace Turbine
{
    public interface ICommandLineOptions
    {
        string Config { get; set; }
        bool Watch { get; set; }
    }

    public class CommandLineOptions : ICommandLineOptions
    {
        [Option('c', "config", Default = "turbine.yaml", HelpText = "The configuration file.")]
        public string Config { get; set; }

        [Option('w', "watch", Default = false, HelpText = "Watch for changes.")]
        public bool Watch { get; set; }
    }
}