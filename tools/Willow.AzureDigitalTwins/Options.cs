using CommandLine;
using System;

namespace Willow.AzureDigitalTwins.BackupRestore
{
    public class Options
    {
        [Option(HelpText = "ADT instance host name.", Required = true)]
        public string AdtInstance { get; set; }

        [Option(Default = false, HelpText = "Command to export twins and relationships from ADT instance.", Group = "Actions")]
        public bool Export { get; set; }

		[Option(Default = false, HelpText = "Command to clear ADT instance.", Group = "Actions")]
		public bool Clear { get; set; }

		[Option(Default = false, HelpText = "Command to import twins and relationships to ADT instance.", Group = "Actions")]
        public bool Import { get; set; }

        [Option(Default = false, HelpText = "Indicates if export should be created with hirarchical folder structure. For import indicates if source package is structured.")]
        public bool Structured { get; set; }

        [Option(Default = false, HelpText = "Command to report stats from ADT instance.", Group = "Actions")]
        public bool Stats { get; set; }

        [Option(HelpText = "Output directory to store output package.")]
        public string OutputDirectory { get; set; }

        [Option(Default = false, HelpText = "On export it indicates if package should be zipped, on import it indicates if input file is zipped.")]
        public bool Zipped { get; set; }

        [Option(HelpText = "If zipped it must point to zip file, if unzipped it must point to a directory containing import folders.")]
        public string ImportSource { get; set; }

        [Option(Default = false, HelpText = "Indicate if models should be included.")]
        public bool IncludeModels { get; set; }

        [Option(Default = 20, HelpText = "Number of threads used for parallel processing.")]
        public int ProcessingThreads { get; set; }

        [Option(Default = 2, HelpText = "Maximum numbers of retry attempts on import.")]
        public int MaxRetries { get; set; }

        public Uri InstanceUri => new Uri($"https://{AdtInstance}", UriKind.Absolute);
    }
}
