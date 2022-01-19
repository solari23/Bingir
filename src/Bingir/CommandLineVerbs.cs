using CommandLine;

namespace Bingir;

public static class CommandLineVerbs
{
    [Verb("fetch", HelpText = "Fetches and locally caches images from Bing servers.")]
    public class FetchVerbOptions
    {
        [Option(
            'n',
            "count",
            Required = false,
            HelpText = "The number of images to fetch.",
            Default = 1)]
        public int Count { get; set; }

        [Option(
            's',
            "silent",
            Required = false,
            HelpText = "Suppresses output from this operation.",
            Default = false)]
        public bool Silent { get; set; }
    }

    [Verb("latest", HelpText = "Returns the local filepath to the latest cached Bing image.")]
    public class LatestVerbOptions
    {
        [Option(
            'f',
            "fetch",
            Required = false,
            HelpText = "Whether to fetch from Bing servers. If false, only the latest from cache will be returned.",
            Default = false)]
        public bool Fetch { get; set; }

        [Option(
            'x',
            "no-error",
            Required = false,
            HelpText = "When set, suppresses errors that occur during processing this command.",
            Default = false)]
        public bool NoError { get; set; }
    }
}
